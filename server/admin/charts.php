<?php
include("pChart/pData.class");
include("pChart/pChart.class");

function pie($data, $title, $filename, $colors = null) {
	$keys = array();
	foreach($data['keys'] as $k) {
		array_push($keys, wordwrap($k, 20, "\r\n"));
	}

	// Dataset definition 
	$DataSet = new pData;
	$DataSet->AddPoint($data['values'],"Serie1");
	$DataSet->AddPoint($keys,"Serie2");
	$DataSet->AddAllSeries();
	$DataSet->SetAbsciseLabelSerie("Serie2");
	
	// Initialise the graph
	$maxlen = 20;
	$w = 400 + $maxlen * 6;
	$pie = new pChart($w,250);
	$pie->drawFilledRoundedRectangle(2,2,$w-3,247,5,240,240,240);
	$pie->drawRoundedRectangle(0,0,$w-1,249,5,230,230,230);
	if ($colors == null) {
		$pie->loadColorPalette('chartcolors.txt', ',');
	} else {
		for($i = 0; $i < count($colors); $i++) {
			$a = $colors[$i];
			$pie->setColorPalette($i, $a[0], $a[1], $a[2]);
		}
	}

	// Draw the pie chart
	$pie->setFontProperties("Fonts/consola.ttf",8);
	$pie->AntialiasQuality = 0;
	$pie->drawPieGraph($DataSet->GetData(),$DataSet->GetDataDescription(),200,110,110,PIE_PERCENTAGE_LABEL,FALSE,50,20,5);
	$pie->drawPieLegend(350,15,$DataSet->GetData(),$DataSet->GetDataDescription(),250,250,250);

	// Write the title
	$pie->setFontProperties("Fonts/MankSans.ttf",10);
	$pie->drawTitle(10,20,$title,100,100,100);

	$pie->Render($filename);	
}

function stacked($data, $title, $filename) {

	// Dataset definition 
	$DataSet = new pData;
	$values = $data['values'];
	
	$bar = new pChart(1040,230);
	
	for ($i = 0; $i < count($values); $i++) {
		$DataSet->AddPoint($values[$i], "Serie".($i+1));
		if ($i != 0 && $i != count($values) - 1) {
			$DataSet->AddSerie("Serie".($i+1));
		}
	}
	$DataSet->SetAbsciseLabelSerie("Serie1");
	$DataSet->SetXAxisFormat("date");
	$DataSet->SetYAxisFormat($data['yformat']);

	// Initialise the graph
	$bar->setDateFormat("M.d");	
	$bar->setFontProperties("Fonts/consola.ttf",8);
	$bar->setGraphArea(80,30,1020,200);
	$bar->drawFilledRoundedRectangle(2,2,1037,227,5,240,240,240);
	$bar->drawRoundedRectangle(0,0,1039,229,5,230,230,230);
	$bar->loadColorPalette('chartcolors.txt', ',');
	$bar->drawGraphArea(255,255,255,TRUE);
	$bar->drawScale($DataSet->GetData(),$DataSet->GetDataDescription(),SCALE_ADDALL,150,150,150,TRUE,0,2,TRUE);
	$bar->drawGrid(4,TRUE,230,230,230,50);

	// Draw the 0 line
	$bar->setFontProperties("Fonts/consola.ttf",6);
	$bar->drawTreshold(0,143,55,72,TRUE,TRUE);

	// Draw the bar graph
	$bar->drawStackedBarGraph($DataSet->GetData(),$DataSet->GetDataDescription(),100);
	
	$bar->setFontProperties("Fonts/tahoma.ttf",8);
	$bar->writeValues($DataSet->GetData(),$DataSet->GetDataDescription(),array("Serie".count($values)));

	// Finish the graph
	$bar->setFontProperties("Fonts/MankSans.ttf",10);
	$bar->drawTitle(10,20,$title,100,100,100);
	
	$bar->Render($filename);
}