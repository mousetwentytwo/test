<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/participation.png';
	
	$query = "select participates, count(participates) as count from
(select x.client_id, ifnull(c.participates,1) as participates from 
(select client_id from godspeed_clients c
union select client_id from godspeed_stats s) x
left join godspeed_clients c on c.client_id = x.client_id) y
group by participates";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$keys = array('Yes', 'No');
	$values = array(0, 0);
	$colors = array(
		array(69,197,229),
		array(251,131,53)
	);

	while ($row = mysql_fetch_assoc($rs)) {
		switch ($row['participates']) {
			case 0:
				$values[1] = $row['count'];
				break;
			case 1:
				$values[0] = $row['count'];
				break;
		}
	} 
	$data = array(
		"keys" => $keys, 
		"values" => $values
	);	
	
	$title = 'Participation';

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