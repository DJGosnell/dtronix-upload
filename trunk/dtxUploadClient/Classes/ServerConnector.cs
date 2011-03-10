﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using dtxCore.Json;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.IO;
using dtxCore;
using System.Web;
using System.Diagnostics;

namespace dtxUpload {

	public delegate void D_UploadProgressChanged(DC_UploadProgressChangedEventArgs e);

	/// <summary>
	/// Class to setup two way communication between HTTP server and client.
	/// </summary>
	public partial class ServerConnector {


		public short max_concurrent_connections;
		//private short current_connections = 0;

		private delegate string GetStringDelegate(Uri address);
		private delegate string PostFileStreamDelegate(Uri address, FileStream stream);

		public D_UploadProgressChanged upload_progress_changed;

		public  DC_ServerInformation server_info;
		public DC_UserInformation user_info;
		private ClientActions actions;
		public UploadFileItem upload_control;
		public Dictionary<string, DC_CacheRequest> cache_requests = new Dictionary<string, DC_CacheRequest>();
		private int cache_length = 7;

		private string http_post_boundry;
		private byte[] http_post_boundry_bytes;



		public string getString(Uri address) {
			DtxHttpRequest request = new DtxHttpRequest(address);
			prepareRequest(request);
			request.receive_timeout = 4000;

			return readServerResponse(request);
		}

		private void getStringResult(IAsyncResult result) {
			GetStringDelegate del = result.AsyncState as GetStringDelegate;
			string server_string = del.EndInvoke(result);

			if(server_string == null) {
				Client.form_Login.Invoke((MethodInvoker)Client.form_Login.invalidServer);
				server_info.is_connected = false;
			} else {
				execServerResponse(server_string);
			}
		}

		public string postFileStream(Uri address, FileStream in_stream) {
			if(http_post_boundry_bytes == null) {
				http_post_boundry = "---------------------------" + DateTime.Now.Ticks.ToString("x");
				http_post_boundry_bytes = Encoding.UTF8.GetBytes("\r\n--" + http_post_boundry + "\r\n");
			}
			
			string header = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
			header = string.Format(header, "file", Path.GetFileName(in_stream.Name));
			byte[] header_bytes = Encoding.UTF8.GetBytes(header);


			DtxHttpRequest request = new DtxHttpRequest(address);
			prepareRequest(request);
			request.headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + http_post_boundry);
			request.method = "POST";

			DC_UploadProgressChangedEventArgs upload_args = new DC_UploadProgressChangedEventArgs();
			upload_args.total_bytes_to_send = in_stream.Length;

			request.headers.Add(HttpRequestHeader.ContentLength, ((http_post_boundry_bytes.Length * 2) + in_stream.Length + header_bytes.Length).ToString());

			byte[] buffer = new byte[1024 * 48];
			int read_length = 0;

			request.write(http_post_boundry_bytes, 0, http_post_boundry_bytes.Length);
			request.write(header_bytes, 0, header_bytes.Length);
			

			while((read_length = in_stream.Read(buffer, 0, buffer.Length)) > 0){
				//IAsyncResult result = out_stream.BeginWrite(buffer, 0, read_length, null, null);
				//out_stream.Flush();
				//out_stream.EndWrite(result);


				request.write(buffer, 0, read_length);

				// Update any events with this information
				upload_args.bytes_sent += read_length;
				upload_progress_changed.Invoke(upload_args);
			}

			request.write(http_post_boundry_bytes, 0, http_post_boundry_bytes.Length);

			in_stream.Close();

			return readServerResponse(request);
		}

		private void postFileStreamResult(IAsyncResult result) {
			PostFileStreamDelegate del = result.AsyncState as PostFileStreamDelegate;
			string server_string = del.EndInvoke(result);

			if(server_string == null) {
				Client.form_Login.Invoke((MethodInvoker)Client.form_Login.invalidServer);
				server_info.is_connected = false;
			} else {
				execServerResponse(server_string);
			}
		}


		private string readServerResponse(DtxHttpRequest request) {
			try {

				DtxHttpResponse response = request.getResponse();
				if(response.status_code != HttpStatusCode.OK) {
					Debug.WriteLine("Query[" + request.address.Query + "] Returned Error: (" + response.status_code.GetHashCode() + ")" + response.status_code);
					return null;
				}

				string read_string = response.readString();

				response.close();
				return read_string;
			} catch(Exception e) {
				Debug.WriteLine("Query[" + request.address.Query + "] " + e.Message);
				return null;


			}

			
		}


		/// <summary>
		/// Execute connection request and login.
		/// </summary>
		public void connect() {
			user_info.password_md5 = user_info.password;
			callServerMethod("user_verification");
		}


		/// <summary>
		///  Uses global client information for connecting.
		/// </summary>
		public ServerConnector() {

			actions = new ClientActions(this);
			server_info = Client.server_info;
			user_info = Client.user_info;
			max_concurrent_connections = Client.config.get<short>("serverconnector.concurrent_connections_max");
		}

		/// <summary>
		/// Uses custom connection information.
		/// </summary>
		/// <param name="server">Server information to use.</param>
		/// <param name="user">User connection information to use.</param>
		public ServerConnector(DC_ServerInformation server, DC_UserInformation user){
			actions = new ClientActions(this);
			server_info = server;
			user_info = user;
		}

		/// <summary>
		///  Used only for UploadFileItem control.
		/// </summary>
		public ServerConnector(UploadFileItem up_control) {
			actions = new ClientActions(this);
			server_info = Client.server_info;
			user_info = Client.user_info;
			upload_control = up_control;

			upload_progress_changed += new D_UploadProgressChanged(actions.upload_progress);

			max_concurrent_connections = 1;
		}

		private void prepareRequest(DtxHttpRequest request) {
			StringBuilder sb = new StringBuilder();

			//http_request.KeepAlive = true;
			//http_request.Credentials = System.Net.CredentialCache.DefaultCredentials;
			request.headers.Add(HttpRequestHeader.UserAgent, "dtxUploadClient/0.3");

			if(user_info.session_key != null) {
				sb.Append("session_key=");
				sb.Append(user_info.session_key);
				sb.Append(";");
			}
			if(user_info.username != null) {
				sb.Append("client_username=");
				sb.Append(user_info.username);
				sb.Append(";");
			}
			if(user_info.password_md5 != null) {
				sb.Append("client_password=");
				sb.Append(user_info.password_md5);
				sb.Append(";");
			}

			request.headers.Add(HttpRequestHeader.Cookie, sb.ToString());
		}

		private void web_client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e) {
			upload_control.Invoke((MethodInvoker)upload_control.uploadPostCompleted);

			if(e.Cancelled) {
				actions.upload_canceled();

			}else if(e.Error != null) {
				Client.form_Login.Invoke((MethodInvoker)Client.form_Login.invalidServer);
				server_info.is_connected = false;

			} else {
				execServerResponse(UTF8Encoding.UTF8.GetString(e.Result));
			}		
		}

		public void disconnect() {
			callServerMethod("logout");
		}

		/// <summary>
		/// Request server information to be sent to the client and parsed.
		/// </summary>
		public void getServerInfo() {
			callServerMethod("get_server_info");
		}

		/// <summary>
		/// Cancel all current connection requestes to the server.
		/// </summary>
		public void cancelActions() {
			//int len = web_clients.Count;
			//for(int i = 0; i < len; i++) {
			//	web_clients[i].CancelAsync();
			//}
		}

		/// <summary>
		/// Begin upload of a file to the server.
		/// </summary>
		/// <param name="file_location">Local file location on the client computer.</param>
		/// 
		public void uploadFile(string file_location) {
			Uri uri = buildUri("upload_file");
			FileStream fs = new FileStream(file_location, FileMode.Open, FileAccess.Read, FileShare.Read);
			if(!fs.CanRead) // If we can not read it, what CAN we do?
				return;

			PostFileStreamDelegate post = new PostFileStreamDelegate(postFileStream);
			post.BeginInvoke(uri, fs, new AsyncCallback(postFileStreamResult), post);
		}

		/// <summary>
		/// Execute a method on the server.
		/// </summary>
		/// <param name="method"></param>
		public void callServerMethod(string method) {
			callServerMethod(method, null);
		}

		/// <summary>
		/// Execute a method on the server.
		/// </summary>
		/// <param name="method">Method name to execute on the server.</param>
		/// <param name="arguments">Arguments to pass to the method on the server.</param>
		public void callServerMethod(string method, params string[] arguments) {
			Uri uri = buildUri(method, arguments);
			GetStringDelegate get = new GetStringDelegate(getString);
			get.BeginInvoke(uri, new AsyncCallback(getStringResult), get);
		}


		/// <summary>
		/// Execute a syncronous method on the server.
		/// </summary>
		/// <param name="method">Method name to execute on the server.</param>
		public T callServerMethodSyncronous<T>(string method) {
			return callServerMethodSyncronous<T>(method, null);
		}

		/// <summary>
		/// Execute a syncronous method on the server.
		/// </summary>
		/// <param name="method">Method name to execute on the server.</param>
		/// <param name="arguments">Arguments to pass to the method on the server.</param>
		public T callServerMethodSyncronous<T>(string method, params string[] arguments) {
			return callServerMethodSyncronous<T>(method, false, arguments);
		}

		/// <summary>
		/// Execute a syncronous method on the server.
		/// </summary>
		/// <param name="method">Method name to execute on the server.</param>
		/// <param name="use_cache">True to enable use of cache.  This will not query the server for the next 3 seconds with the same query.</param>
		/// <param name="arguments">Arguments to pass to the method on the server.</param>
		public T callServerMethodSyncronous<T>(string method, bool use_cache, params string[] arguments) {
			Uri uri = buildUri(method, arguments);
			string concat_uri = uri.ToString();

			if(use_cache && cache_requests.ContainsKey(concat_uri)) {
				if(cache_requests[concat_uri].request_time.AddSeconds(cache_length) < DateTime.Now) {
					return (T)cache_requests[concat_uri].data;
				} else {
					cache_requests.Remove(concat_uri);
				}
			}

			//WebClient client = getWebClient();
			string data = "client.DownloadString(uri);";

			if(data == null) {
				return default(T);

			} else {
				try {
					string[] parsed = parseServerData(data);
					JsonReader reader = new JsonReader(parsed[1]);
					T data_deserialized = reader.Deserialize<T>();

					if(data_deserialized == null) {
						return default(T);
					}

					if(use_cache) {
						cache_requests.Add(concat_uri, new DC_CacheRequest() {
							data = data_deserialized,
							request_time = DateTime.Now
						});
					}

					return data_deserialized;
				} catch {
					return default(T);
				}
				
			}
		}


		/// <summary>
		/// Execute a syncronous method on the server.
		/// </summary>
		/// <param name="id">File ID to download</param>
		/// <param name="offset">Offset of file to start to read from</param>
		/// <param name="to_read">Total bytes to read.</param>
		public byte[] partialFileDownloadSyncronous(string id, long offset, uint to_read) {
			Uri uri = buildUri("view_file", id);

			return new byte[224];

			//WebClient client = getWebClient();
			//client.Headers.Set(HttpRequestHeader.Range, "bytes="+ offset + "-" + offset + to_read);
			//return client.DownloadData(uri);
		}

		/// <summary>
		/// Build valid server URI for the requested command.
		/// </summary>
		/// <param name="method">Method to call on the server.</param>
		/// <param name="arguments">Array of string arguments to pass to the method on the server.</param>
		/// <returns>Completed URI, or argument exception.</returns>
		private Uri buildUri(string method, params string[] arguments) {
			StringBuilder sb = new StringBuilder();

			sb.Append(server_info.server_url);
			sb.Append("?action=");
			sb.Append(method);
			if(arguments != null) {
				foreach(string arg in arguments) {
					sb.Append("&args[]=");
					sb.Append(arg);
				}
			}
			try {
				return new Uri(sb.ToString());
			} catch {
				throw new ArgumentException("The build URI for the server was invalid.  This is due to invalid arguments or method called.");
			}


		}

		/// <summary>
		/// Data is passed here from the async request to be parsed and executed.
		/// </summary>
		/// <param name="server_data">Raw data that comes from the server</param>
		private void execServerResponse(string server_data) {
			string[] parsed = parseServerData(server_data);

			if(parsed[0] == null) // If the server responce could not be  parsed, then just quit!
				return;

			MethodInfo requested_method = typeof(ClientActions).GetMethod(parsed[0]);
			if(requested_method != null) {
				if(parsed[1] == null) {
					requested_method.Invoke(actions, null);
				} else {
					requested_method.Invoke(actions, new object[] { parsed[1] });
				}

			} else {
				Client.form_Login.Invoke((MethodInvoker)delegate{
					MessageBox.Show("Server requested invalid method.  This is usually due to an outdated client.\n\nError:\n"+ server_data );
				});
				// Server requested a method that we do not support.  TO DO.
			}
		}

		/// <summary>
		/// Exposed method to allow for global parsing of raw server data.
		/// </summary>
		/// <param name="server_data"></param>
		/// <returns>[0]: Function to call; [1]: Arguments to pass;</returns>
		public string[] parseServerData(string server_data) {
			string[] parsed = new string[2];
			try {
				int function_len = int.Parse(server_data.Substring(0, 3));
				parsed[0] = server_data.Substring(3, function_len);
				parsed[1] = null;

				if(server_data.Length > function_len + 3) {
					parsed[1] = server_data.Substring(function_len + 3);
				}

				return parsed;

			} catch {
				// Server information could not be parsed. TO DO.
				return parsed;
			}
		}
	}
}
