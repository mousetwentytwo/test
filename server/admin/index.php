<html>
	<head>
		<link rel="stylesheet" href="css/admin.css"/>
	</head>
	<body>
		<div class="charts">
			<img src="occurance.php?col=version"/>
			<img src="occurance.php?col=wpf"/>
			<img src="occurance.php?col=os"/>
			<img src="occurance.php?col=culture"/>
			<img src="occurance.php?col=country_name"/>
			<img src="occurance.php?col=exit_code"/>
			<img src="stability.php"/>
			<img src="participation.php"/>
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
	</body>
</html>