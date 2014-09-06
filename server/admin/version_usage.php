<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$name = './charts/usage.png';
	
	$todayminus30 = date('Y-m-d', strtotime("-30 days"));
	
	$query = "select distinct version from godspeed_stats order by version";
	$rs = mysql_query($query);
	$versions = array();
	$blank = array();
	$v = array(array());
	while ($row = mysql_fetch_assoc($rs)) {
		array_push($versions, $row['version']);
		array_push($blank, 0);
		array_push($v, array());
	}
	
	$query = "SELECT UNIX_TIMESTAMP(DATE(date)) as `date`, 
				     version as `version`,
				     COUNT(version) as `clients`
			  FROM `godspeed_stats` M
			  WHERE date >= '$todayminus30'
			  GROUP BY DATE(date), version
			  ORDER BY DATE(date), version";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$lastdate = null;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($lastdate != null) {
			$lastdate += 86400;
			while ($lastdate < $row['date']) {
				array_push($date, $lastdate);
				array_push($v, $blank);
				$lastdate += 86400;
			}
		}

		$c = count($v[0]) - 1;
			if ($c == -1 || $v[0][$c] != $row['date']) {
			array_push($v[0], $row['date']);
			for ($i = 1; $i < count($v); $i++) {
				array_push($v[$i], 0);
			}
			$c++;
		}
		
		$i = 0;
		while ($versions[$i] != $row['version']) $i++;
		$v[$i+1][$c] = $row['clients'];
		
		$lastdate = $row['date'];
	} 
	
	$data = array(
		"keys" => $versions,
		"values" => $v
	);
	
	$title = 'Version usage';

	if (!$debug) {
		stacked2($data, $title, $name);
		$fp = fopen($name, 'rb'); 
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		print_r($data);
	}

	exit;
?>