<? // ### COPIES GRAPHICS FROM REMOTE SERVERS ASSUMING THEY USE ORDERED NAMING MECHANIC
//include_once ('funcs.ini.php');
//CON ();
for ($x = 1; $x <= 493; ++$x) {
  $val = $x;
  $location = "http://beta.veekun.com/dex/media/overworld/right/frame2/" . $x . ".png";
  $file = $x . ".png";
  if(!@copy($location, $file)) {
    /*
	  ### UPDATES THE DATABASE WITH APPROPRIATE VALUE FOR IF SPRITE IS AVAILABLE...
	$updateq = "SELECT m.monster_ntnl_num, a.art_relator FROM pkmn_monster m, pkmn_art a WHERE m.monster_ntnl_num = '" . $val . "' AND a.art_relator = m.monster_relator";
	if ($updater = mysql_query ($updateq)) {
	  while ($updatero = mysql_fetch_row ($updater)) {
	    $ntnl = $updatero[0];
		$rel = $updatero[1];
		mysql_query ("UPDATE pkmn_art SET art_ranger2 = 'n' WHERE art_relator = '" . $rel . "'") or die (mysql_error ());
	  }
	} */
    $errors = error_get_last();
    echo "ERROR OCCURED: " . $errors['type'] . " - " . $errors['message'] . ".<br />&nbsp;&nbsp;&nbsp;&nbsp;<strong>--&gt;</strong>" . $rel . " was changed to 'n'<br />";
  } else echo $file . " was successfully copied from " . $location . "<br />"; 
} ?>