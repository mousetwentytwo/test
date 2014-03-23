<?php
	$debug = isset($_GET['debug']);
	if ($debug) {
		ini_set("display_errors", "On");
		ini_set("error_reporting", 2047);
	}

	include("../db.php");
	$query = "SELECT country_code, count(country_code) as num FROM `godspeed_stats` group by country_code ORDER BY COUNT(country_code) DESC";
	$rs = mysql_query($query);
	if (!mysql_query($query)) {
		echo mysql_error();
	}
	
?>
<html>
	<head>
		<script type="text/javascript" src="js/jquery-2.1.0.min.js"></script>
		<script type="text/javascript" src="js/jquery-jvectormap-1.2.2.min.js"></script>
		<script type="text/javascript" src="js/jquery-jvectormap-world-mill-en.js"></script>
		<link rel="stylesheet" href="css/jquery-jvectormap-1.2.2.css"/>
	</head>
	<body>
		<div id="world-map"></div>
		<script type="text/javascript">
			$(function(){
			  var data = {
				<?php
					while ($row = mysql_fetch_assoc($rs)) {
						printf('"%s": %s, ', $row['country_code'], $row['num']);
					}
				?>
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