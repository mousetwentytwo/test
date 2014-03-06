<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/recognition.png';
	
	$todayminus30 = date('Y-m-d', strtotime("-30 days"));
	
	$query = "SELECT date, 
					 COUNT(client_id), 
					 SUM(fully) as `fully`, 
					 SUM(partially) as `partially` 
			  FROM (SELECT UNIX_TIMESTAMP(DATE(date)) as `date`, 
				 		   client_id, 
						   MAX(games_recognized) as `fully`, 
						   MAX(partially_recognized) as `partially` 
				   FROM `godspeed_stats`
				   WHERE date >= '$todayminus30'
				   GROUP BY DATE(date), client_id
				   ORDER BY DATE(date)) S
			  GROUP BY date";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$date = array();
	$total = array();
	$fully = array();
	$partially = array();

	$lastdate = null;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($lastdate != null) {
			$lastdate += 86400;
			while ($lastdate < $row['date']) {
				array_push($date, $lastdate);
				array_push($total, 0);
				array_push($fully, 0);
				array_push($partially, 0);
				$lastdate += 86400;
			}
		}
		array_push($date, $row['date']);
		array_push($total, ($row['fully'] + $row['partially']));
		array_push($fully, $row['fully']);
		array_push($partially, $row['partially']);
		$lastdate = $row['date'];
	} 
	
	$data = array(
		"yformat" => "number",
		"values" => array($date, $fully, $partially, $total)
	);	
	
	$title = 'Game recognitions';

	stacked($data, $title, $name);

	$fp = fopen($name, 'rb'); 
	
	if (!$debug) {
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		print_r($data);
	}

	exit;
?>