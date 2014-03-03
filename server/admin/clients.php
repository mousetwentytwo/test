<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/clients.png';
	
	//TODO: last 30 days
	$query = "SELECT UNIX_TIMESTAMP(date) as `date`,
				     COUNT(`client_id`) as `totalusers`,
				     SUM(`new`) as `newusers`
			  FROM (SELECT DATE(M.date) as `date`,
				           M.client_id,
				           COUNT(*) as `num`,
				          (SELECT IF(DATE(MIN(date)) = DATE(M.date), 1, 0) FROM `godspeed_stats` X WHERE X.client_id = M.client_id) as `new`
			        FROM `godspeed_stats` M
				    GROUP BY M.date, M.client_id
					ORDER BY DATE(M.date)) S
			  GROUP BY date";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$date = array();
	$totalusers = array();
	$newusers = array();
	$diff = array();

	$lastdate = null;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($lastdate != null) {
			$lastdate += 86400;
			while ($lastdate < $row['date']) {
				array_push($date, $lastdate);
				array_push($diff, 0);
				array_push($newusers, 0);
				array_push($totalusers, 0);
				$lastdate += 86400;
			}
		}
		array_push($date, $row['date']);
		array_push($diff, ($row['totalusers'] - $row['newusers']));
		array_push($newusers, $row['newusers']);
		array_push($totalusers, $row['totalusers']);
		$lastdate = $row['date'];
	} 
	
	//TODO: reduce 30 days
	$data = array(
		"yformat" => "number",
		"values" => array($date, $diff, $newusers, $totalusers)
	);	
	
	$title = 'User count';

	stacked($data, $title, $name, false);

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