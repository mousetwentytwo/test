<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/stability.png';
	
	$w = '';
	if (isset($_GET['version'])) {
		$w = "where version like '".$_GET['version']."%'";
	}
	
	$query = "SELECT exit_code AS `key`, COUNT(exit_code) AS `value` FROM `godspeed_stats` $w GROUP BY exit_code ORDER BY COUNT(exit_code) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$keys = array('OK', 'Error');
	$values = array(0, 0);
	$colors = array(
		array(69,197,229),
		array(251,131,53)
	);

	while ($row = mysql_fetch_assoc($rs)) {
		if ($row['key'] == 0) {
			$values[0] += $row['value'];
		} else {
			$values[1] += $row['value'];
		}
	} 
	$data = array(
		"keys" => $keys, 
		"values" => $values
	);	
	
	$title = 'Stability';
	if (isset($_GET['version'])) {
		$title .= ' (Version '.$_GET['version'].')';
	}

	pie($data, $title, $name, $colors);

	$fp = fopen($name, 'rb'); 
	
	if (!$debug) {
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		print_r($data);
	}
?>