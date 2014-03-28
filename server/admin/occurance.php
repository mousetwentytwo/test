<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	include("charts.php");

	$col = $_GET['col'];
	$crop = isset($_GET['crop']) ? $_GET['crop'] : 0;
	$name = "./charts/$col.png";
	
	$column = isset($_GET['crop']) ? sprintf('substring(`%s`, 1, %s)', $col, $_GET['crop']) : $col;
	
	$query = "SELECT $column AS `key`, COUNT(*) AS `value` FROM `godspeed_stats` GROUP BY $column ORDER BY COUNT(*) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}
	
	$keyr = array();
	if (isset($_GET['keyr'])) {
		$f = file($_GET['keyr']);
		for ($i = 0; $i < count($f); $i++) {
			$x = explode('|', trim($f[$i]));
			$keyr[$x[0]] = $x[1];
		}
	}

	$keys = array();
	$values = array();

	$j = 0;
	$o = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($j < 7) {
			$k = $row['key'];
			if ($k == null || $k == '') $k = '<empty>';
			if (isset($keyr[$k])) $k = $keyr[$k];
			array_push($keys, $k);
			array_push($values, $row['value']);
			$j++;
		} else {
			$o += $row['value'];
		}
	} 
	if ($o > 0) {
		array_push($keys, 'Others');
		array_push($values, $o);
	}
	
	$data = array(
		"keys" => $keys, 
		"values" => $values
	);
	
	
	$title = '';
	switch ($col) {
		case 'version':
			$title = 'Application versions';
			break;
		case 'wpf':
			$title = 'Framework versions';
			break;
		case 'os':
			$title = 'OS versions';
			break;
		case 'country':
			$title = 'Countries';
			break;
		case 'culture':
			$title = 'Cultures';
			break;
		case 'exit_code':
			$title = 'Exit codes';
			break;
	}

	pie($data, $title, $name);

	$fp = fopen($name, 'rb'); 
	
	if (!$debug) {
		header("Content-Type: image/png");
		header("Content-Length: " . filesize($name));
		fpassthru($fp);
	} else {
		echo $query;
		print_r($data);
	}

	exit;
?>