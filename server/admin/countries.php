<?php
	include('countries-inner.php');
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
			<h1><?php printf("%s countries<br/>%s users (+%s unknown)", $count, $sum, $all-$sum); ?></h1>
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