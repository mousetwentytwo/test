<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	
	$query = "SELECT command, SUM(count) as `count` FROM `godspeed_command_usage` GROUP BY command ORDER BY SUM(count) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	echo '<h1>Commands</h1>';
	echo '<table><tr><th>Command</th><th>Count</th></tr>';
	while ($row = mysql_fetch_assoc($rs)) {
		printf('<tr><td>%s</td><td align="center">%s</td></tr>', $row['command'], $row['count']);
	} 
	echo '</table>';
?>