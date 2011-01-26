<?php

include_once("./../../functions.php");
?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
	"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
	<title></title>
	<!-- Core Scripts -->
	<script src="js/mootools-core-1.3-full-nocompat.js" type="text/javascript"></script>

	<!-- Section Scripts -->
	<script src="js/file_manager.js" type="text/javascript"></script>
	
	<!-- Styles -->
	<link rel="stylesheet" type="text/css" href="default.css" />
	
	<style type="text/css">	
		
	body{
		background-color: #7d98ab;
		background-image: url('images/border_top.png');
		background-repeat: repeat-x;
		background-position: top left;
	}
	
	#uploads_content_table{
		font-size: 13px;
	}
	
	.folder_container{
		color: #ffffff;
		border-bottom: 1px solid #e2e2e2;
		width: 99%;
		margin-left: 5px;
		margin-top: 10px;
		text-transform:capitalize;
	}
	
	.nav_links{
		text-decoration: none;
		color: #ffffff;
		margin-left: 10px;
	}
	
	.nav_links:hover{
		text-decoration: underline;
	}
	
	.item_upload:hover{
		background-color: #657f91;
		cursor: default;
	}
	.item_upload-selected{
		background-color: #4c6a7f;
		background-repeat: repeat-x;
	}
	
	.item_upload{
		color: #dddddd;
	}
	
	a:visited{
		color: #ffffff;
		text-decoration: none;
	}
	a:active{
		color: #eaeaea;
		text-decoration: underline;
	}
	a:hover{
		color: #eaeaea;
		text-decoration: underline;
	}
	a{
		color: #eaeaea;
		text-decoration: none;
	}
	.folder_colapse{
		cursor: pointer;
		font-size: 20px;
	}
	#properties{
		position: absolute;
		bottom: -60px;
		left: 0px;
		right: 0px;
		height: 60px;
		background: #383838;
		border-top: 1px solid #ffffff;
	}
	
	.properties_details{
		position: relative;
		float: left;
		color: #e6e6e6;
		margin-right: 20px;
		margin-top: 10px;
	}
	
	
	</style>
</head>
<body>

<!-- Begin Main Content List -->
<div style="overflow: auto; position: absolute; top: 0px; bottom: 50px; right: 0px; left: 0px; float: left;" id="content_items_wrapper">
	<table id="uploads_content_table" width="100%" cellpadding="0" cellspacing="0">
		<thead>
			<tr style="background-color: #e1e1e1;">
				<td width="20"></td>
				<td width="30"></td>
				<td>Filename</td>
				<td width="90">Status</td>
				<td width="120">Upload Date</td>
				<td width="80">Size</td>
				<td width="40">Views</td>
				<td width="20"></td>
			</tr>
		</thead>
		<tbody style="color: #e0e0e0;" valign="middle" id="uploaded_items_list">
			<tr id="folder_id_default" style="display:none;">
				<td colspan="5">
					<div class="folder_container">
						<span class="folder_colapse">-&nbsp;</span>
						<span class="folder_name"></span>
					</div>
				</td>
				<td colspan="5">
					<a href="#" class="nav_links">&laquo;Previous</a>
					<a href="#" class="nav_links">Next&raquo;</a>
				</td>
			</tr>
			<tr id="item_id_" class="item_upload" style="display:none;">
				<td><input type="checkbox" /></td>
				<td><img src="images/dtxUploadLogo.png" style="width: 20px; height: 20px;" /></td>
				<td><a href=""></a></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
			</tr>
		</tbody>
	</table>
</div>
<!-- End Main Content List -->

<!-- Begin Selection Properties -->
<div id="properties">
	<div style="float: left; margin-right: 20px;">
		<img src="images/dtxUploadLogo.png" style=" float: left; width: 40px; height: 40px; border: 1px solid #ffffff; position: relative; top: 4px; left: 3px;" id="properties_image"/>
		<div style="color: #ffffff; font-size: 20px; position: relative; float: left; left: 7px; margin-right: 40px;" id="properties_header">15 Selected Items</div><br />
		<div style="position: relative; left: 10px; width: 280px; z-index: 2;">
			<button id="properties_buton_selectnone">Select None</button>
			<select id="properties_actions">
				<option value="" disabled="disabled" selected="selected">Actions</option>
				<option value="make_public">Make Public</option>
				<option value="make_private">Make Private</option>
				<option value="" disabled="disabled">- - -</option>
				<option value="delete">Delete</option>
			</select>
		</div>
	</div>
	<div style="position: relative; float: left;" id="properties_details_container">
	
	</div>

</div>
<!-- End Begin Selection Properties -->

	
</body>
</html>