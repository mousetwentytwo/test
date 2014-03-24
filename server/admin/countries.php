<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	$query = "SELECT country_code, country_name, count(*) as num FROM `godspeed_stats` group by country_code, country_name ORDER BY COUNT(*) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}
	
	$mapData = '';
	$tableData = '';
	$count = 0;
	while ($row = mysql_fetch_assoc($rs)) {
		$mapData .= sprintf('"%s": %s, ', $row['country_code'], $row['num']);
		$tableData .= sprintf('<tr><td>%s</td><td>%s</td></tr>', $row['country_name'], $row['num']);
		$count++;
	}
	
?>
<html>
	<head>
		<script type="text/javascript" src="js/jquery-2.1.0.min.js"></script>
		<script type="text/javascript" src="js/jquery-jvectormap-1.2.2.min.js"></script>
		<script type="text/javascript" src="js/jquery-jvectormap-world-mill-en.js"></script>
		<link rel="stylesheet" href="css/jquery-jvectormap-1.2.2.css"/>
		<link rel="stylesheet" href="css/admin.css"/>
	</head>
	<body>
		<div id="world-map"></div>
		<div class="list">
			<h1><?php echo $count; ?></h1>
			<table>
				<?php echo $tableData; ?>
			</table>
		</div>
		<script type="text/javascript">
			$(function(){
			  var data = {
				<?php echo $mapData; ?>
			  };
			  var container = $('#world-map');
			  container.vectorMap({
				map: 'world_mill_en',
				series: {
				  regions: [{
					values: data,
					scale: ['#C8EEFF', '#0071A4'],
					normalizeFunction: 'polynomial'
				  }]
				},
				onRegionLabelShow: function(e, el, code){
				  if (data[code]) {
					el.html(el.html()+' ('+data[code]+')');
				  }
				}
			  });
			  // var map = container.vectorMap('get','mapObject');
			  
			  // for (var name in data) {
				// for (var code in map.mapData.paths) {
					// if (map.mapData.paths[code].name == name) {
						// values[code] = data[name];
						// break;
					// }
				// }
			  // }
			  
			  // map.series.regions[0].setValues(values);
			});
		</script>
	</body>
</html>