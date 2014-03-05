<html>
	<head>
		<style type="text/css">
		
		.list table {
			border-collapse: collapse;
			border-spacing: 0;
			margin:0px;
			padding:0px;
		}
		
		.list tr:nth-child(odd) { 
			background-color:#e5e5e5; 
		}
		.list tr:nth-child(even) { 
			background-color:#ffffff; 
		}
		
		.list td {
			vertical-align:middle;
			padding:7px;
			font-size:10px;
			font-family:Arial;
			font-weight:normal;
			color:#000000;
		}
		
		.list th {
			background-color:#4c4c4c;
			border-width:0px 0px 1px 1px;
			text-align: left;
			padding: 7px;
			font-size:14px;
			font-family:Verdana;
			font-weight:bold;
			color:#ffffff;
		}
		
		</style>
	</head>
	<body>
		<div class="charts">
			<img src="occurance.php?col=version"/>
			<img src="occurance.php?col=wpf"/>
			<img src="occurance.php?col=os"/>
			<img src="occurance.php?col=culture"/>
			<img src="occurance.php?col=country"/>
			<img src="occurance.php?col=exit_code"/>
			<img src="stability.php"/>
			<img src="usage.php"/>
			<img src="clients.php"/>
			<img src="recognition.php"/>
		</div>
		<div class="list">
			<?php
				include('commands.php');
			?>
		</div>
	</body>
</html>