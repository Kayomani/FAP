<?php
$version = '';
  if(isset($_GET["v"]))
     $version = $_GET["v"];
     switch($version)
     {
       case "FAP Alpha 5":
       echo "Your client is out of date.\n Latest client: Beta 1";
          break;
       case "FAP Beta 1":
          echo "Your client is up to date.";
          break;
       default:
          echo "Unknown client version please go to\n http://code.google.com/p/fap/ to get the latest client.";
     }
?>