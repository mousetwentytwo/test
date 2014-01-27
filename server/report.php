<?php
    ini_set("display_errors", "On");
    ini_set("error_reporting", 2047);
	if ($_SERVER['HTTP_USER_AGENT'] != 'GODspeed') exit;

	$files = scandir('./issues');
	$last = $files[count($files) - 1];

	if ($last == '..') $last = 0;
	
	$issue = ++$last;
	$filename = './issues/'.$issue;
	echo $filename;
	
	$f = fopen($filename, 'w');
	fwrite($f, $HTTP_RAW_POST_DATA);
	fclose($f);
	
	if (!mail('mercenary@gabber.hu', 'GODspeed issue #'.$issue, $HTTP_RAW_POST_DATA)) {
		echo 'No mail';
	}
?>