<?php
    ini_set("display_errors", "On");
    ini_set("error_reporting", 2047);
	if ($_SERVER['HTTP_USER_AGENT'] != 'GODspeed') exit;

	$files = scandir('./issues');
	$last = $files[count($files) - 1];

	if ($last == '..') $last = 0;
	$issue = ++$last;
	
    $separator = md5(time());
    $eol = PHP_EOL;
	
	$headers = '';
	$body = '';
	
	if (isset($_POST['log'])) {

		// main header (multipart mandatory)
		$headers = "From: GODspeed Error Reporting <anonymous@ghost.hu>" . $eol;
		$headers .= "MIME-Version: 1.0" . $eol;
		$headers .= "Content-Type: multipart/mixed; boundary=\"" . $separator . "\"" . $eol . $eol;
		$headers .= "Content-Transfer-Encoding: 7bit" . $eol;
		$headers .= "This is a MIME encoded message." . $eol . $eol;

		// message
		$f = fopen('./issues/'.$issue, 'w');
		fwrite($f, $_POST['log']);
		fclose($f);
		
		$headers .= "--" . $separator . $eol;
		$headers .= "Content-Type: text/plain; charset=\"utf-8\"" . $eol;
		$headers .= "Content-Transfer-Encoding: 8bit" . $eol . $eol;
		$headers .= $_POST['log'] . $eol . $eol;

		// attachment
		foreach ($_FILES as $k => $v) {
			$filename = $issue.'_'.$k.'.png';
			$file = './screens/'.$filename;
			move_uploaded_file($v['tmp_name'], $file);
			$file_size = filesize($file);
			$handle = fopen($file, "r");
			$content = fread($handle, $file_size);
			fclose($handle);
			$content = chunk_split(base64_encode($content));
			
			$headers .= "--" . $separator . $eol;
			$headers .= "Content-Type: application/octet-stream; name=\"" . $filename. "\"" . $eol;
			$headers .= "Content-Transfer-Encoding: base64" . $eol;
			$headers .= "Content-Disposition: attachment" . $eol . $eol;
			$headers .= $content . $eol . $eol;
		}
		
		$headers .= "--" . $separator . "--";	
	} else {
		$f = fopen('./issues/'.$issue, 'w');
		fwrite($f, $HTTP_RAW_POST_DATA);
		fclose($f);
		$body = $HTTP_RAW_POST_DATA;
	}
	
	if (!mail('mercenary@gabber.hu', 'GODspeed issue #'.$issue, $body, $headers)) {
		echo 'No mail';
	}
?>