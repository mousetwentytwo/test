<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	$query = "SELECT country_code, country_name, count(distinct client_id) as num, (SELECT s.date FROM `godspeed_stats` s WHERE s.country_code = m.country_code ORDER BY s.date ASC LIMIT 0,1) as joined FROM `godspeed_stats` m group by country_code, country_name ORDER BY COUNT(distinct client_id) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}
	
	$mapData = '';
	$tableData = '';
	$count = 0;
	$sum = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		$sum += $row['num'];
		$mapData .= sprintf('"%s": %s, ', $row['country_code'], $row['num']);
		$tableData .= sprintf('<tr><td>%s</td><td>%s</td><td>%s</td></tr>', $row['country_name'], $row['num'], $row['joined']);
		$count++;
	}
	
	$query = "select count(*) as count from
				(select x.client_id, ifnull(c.participates,1) as participates from 
				(select client_id from godspeed_clients c
				union select client_id from godspeed_stats s) x
				left join godspeed_clients c on c.client_id = x.client_id) y";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	$row = mysql_fetch_assoc($rs);
	$all = $row['count'];
	
?>