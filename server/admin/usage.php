<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/usage.png';
	
	//TODO: last 30 days
	$query = "SELECT UNIX_TIMESTAMP(DATE(date)) as `date`, 
				     SUM(TIME_TO_SEC(`usage`)) as `usage`,
				     SUM(TIME_TO_SEC(`transfer_time`)) as `transfer`,
			 	     (SELECT COUNT(DISTINCT client_id) FROM `godspeed_stats` WHERE DATE(date) = DATE(M.date)) as `clients`
			  FROM `godspeed_stats` M
			  GROUP BY DATE(date) 
			  ORDER BY DATE(date) 
			  LIMIT 0, 30";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$date = array();
	$usage = array();
	$transfer = array();
	$diff = array();

	$lastdate = null;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($lastdate != null) {
			$lastdate += 86400;
			while ($lastdate < $row['date']) {
				array_push($date, $lastdate);
				array_push($diff, 0);
				array_push($transfer, 0);
				array_push($usage, 0);
				$lastdate += 86400;
			}
		}
		array_push($date, $row['date']);
		array_push($diff, ($row['usage'] - $row['transfer']) / $row['clients']);
		array_push($transfer, $row['transfer'] / $row['clients']);
		array_push($usage, $row['usage'] / $row['clients']);
		$lastdate = $row['date'];
	} 
	
	//TODO: reduce 30 days
	$data = array(
		"yformat" => "number",
		"values" => array($date, $diff, $transfer, $usage)
	);	
	
	$title = 'Average usage time';

	if (!$debug) {
		stacked($data, $title, $name, false);
		$fp = fopen($name, 'rb'); 
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		print_r($data);
	}

	exit;
?>