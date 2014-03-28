<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}
	
   function ToTime($Value)
    {
     $Hour   = floor($Value/3600);
     $Minute = floor(($Value - $Hour*3600)/60);
     $Second = floor($Value - $Hour*3600 - $Minute*60);

     if (strlen($Hour) == 1 )   { $Hour = "0".$Hour; }
     if (strlen($Minute) == 1 ) { $Minute = "0".$Minute; }
     if (strlen($Second) == 1 ) { $Second = "0".$Second; }

     return($Hour.":".$Minute.":".$Second);
    }	

	include("../db.php");
	
// select client_id, country_code, count(client_id) as `count`, SUM(TIME_TO_SEC(`usage`)) as `usage` from godspeed_stats
// group by client_id
// order by count(client_id) desc	
	
	$query = "select runs, count(runs) as `num`, sum(`sum`) as `sum`, max(`max`) as `max`, sum(`tsum`) as `tsum`, max(`tmax`) as `tmax`, avg(`avg`) as `avg`, max(`tmost`) as `tmost`, sum(`errors`) as `errors` from
	 (select client_id, count(client_id) as `runs`, 
	 	 SUM(TIME_TO_SEC(`usage`)) as `sum`, MAX(TIME_TO_SEC(`usage`)) as `max`,
	 	 SUM(TIME_TO_SEC(`transfer_time`)) as `tsum`, MAX(TIME_TO_SEC(`transfer_time`)) as `tmax`,
	 	 AVG(TIME_TO_SEC(`transfer_time`) * 100 / TIME_TO_SEC(`usage`)) as `avg`, MAX(TIME_TO_SEC(`transfer_time`) * 100 / TIME_TO_SEC(`usage`)) as `tmost`,
		 (select count(*) from godspeed_stats where client_id = s.client_id and exit_code != 0) as `errors`
	 from godspeed_stats s
	  group by client_id) as x
  group by runs
  order by runs desc";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}

	echo '<h1>Loyalty</h1>';
	echo '<table><tr><th>Runs</th><th>Users</th><th>Avg.usage</th><th>Max usage</th><th>Avg.transfer</th><th>Max transfer</th><th>Avg.efficiency</th><th>Max efficiency</th><th>Errors</th></tr>';
	while ($row = mysql_fetch_assoc($rs)) {
		printf('<tr><td>%s</td><td align="center">%s</td><td align="center">%s</td><td align="center">%s</td><td align="center">%s</td><td align="center">%s</td><td align="center">%01.2f%%</td><td align="center">%01.2f%%</td><td align="center">%01.2f%%</td></tr>', $row['runs'], $row['num'], ToTime($row['sum'] / $row['num'] / $row['runs']), ToTime($row['max']), ToTime($row['tsum'] / $row['num'] / $row['runs']), ToTime($row['tmax']), $row['avg'], $row['tmost'], $row['errors'] * 100 / $row['runs'] / $row['num']);
	} 
	echo '</table>';
?>