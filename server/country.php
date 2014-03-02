<?php

 // Standard inclusions   
 include("pChart/pData.class");
 include("pChart/pChart.class");
 include("db.php");
 include("charts.php");
 
 $col = $_GET['col'];
 
 $data = occurance($col);
 //print_r($data); 

 $name = './charts/$col.png';
 
 //UNDONE
 if ($)
 pie($data, "Countries", $name);
 

$fp = fopen($name, 'rb'); 

// send the right headers
header("Content-Type: image/png");
header("Content-Length: " . filesize($name));

// dump the picture and stop the script
fpassthru($fp);
exit; 
?>