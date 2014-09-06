<html>
	<head>
		<link rel="stylesheet" href="css/admin.css"/>
	</head>
	<body>
		<div class="charts">
			<img src="occurance.php?col=version"/>
			<img src="occurance.php?col=wpf"/>
			<img src="occurance.php?col=os&crop=29&keyr=os.txt"/>
			<img src="occurance.php?col=culture"/>
			<img src="occurance.php?col=country_name"/>
			<img src="servers.php"/>
			<img src="stability.php"/>
			<img src="stability.php?version=1.1"/>
			<img src="participation.php"/>
			<img src="install.php"/>
			<img src="version_usage.php"/>
			<img src="usage.php"/>
			<img src="clients.php"/>
			<img src="recognition.php"/>
			<img src="transfer.php"/>
		</div>
		<div class="list">
			<?php
				include('commands.php');
			?>
		</div>
		<div class="list">
			<?php
				include('loyalty.php');
			?>
		</div>
		<div class="list">
		<?php
			include('countries-inner.php');
			printf("<h1>%s countries<br/>%s users (+%s unknown)</h1>", $count, $sum, $all-$sum);
			printf('<table><tr><th>Country</th><th>Users</th><th>Joined</th></tr>%s</table>', $tableData);
		?>
	</body>
</html>