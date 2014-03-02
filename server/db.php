<?php

$host = "localhost"; 
$user = "mercenary"; 
$pass = "hardcore"; 

$r = mysql_connect($host, $user, $pass);
mysql_select_db('mercenary');

function occurance($col) {
	$query = "SELECT $col AS `key`, COUNT($col) AS `value` FROM `godspeed_stats` GROUP BY $col ORDER BY COUNT($col) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$keys = array();
	$values = array();

	$j = 0;
	$o = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		if ($j < 7) {
			array_push($keys, $row['key']);
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
	return array($keys, $values);
}

?>