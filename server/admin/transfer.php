<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/transfer.png';
	
	$todayminus30 = date('Y-m-d', strtotime("-30 days"));
	
	$query = "SELECT UNIX_TIMESTAMP(DATE(date)) as `date`, 
					 SUM(transferred_bytes) as `bytes`
			  FROM `godspeed_stats` M
			  WHERE date >= '$todayminus30'
			  GROUP BY DATE(date) 
			  ORDER BY DATE(date)";
			  
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$date = array();
	$bytes = array();

	$lastdate = null;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($lastdate != null) {
			$lastdate += 86400;
			while ($lastdate < $row['date']) {
				array_push($date, $lastdate);
				array_push($bytes, 0);
				$lastdate += 86400;
			}
		}
		array_push($date, $row['date']);
		array_push($bytes, $row['bytes']);
		$lastdate = $row['date'];
	} 
	
	$data = array(
		"yformat" => "metric",
		"values" => array($date, $bytes, $bytes)
	);	
	
	$title = 'Bytes transferred';
	
	if (!$debug) {
		stacked($data, $title, $name);
		$fp = fopen($name, 'rb'); 
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		print_r($data);
	}

	exit;
?>