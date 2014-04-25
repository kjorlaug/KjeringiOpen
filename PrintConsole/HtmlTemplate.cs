using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintConsole
{
    public class HtmlTemplate
    {
        public const String Template = @"<html>
<head>
    <meta http-equiv='Content-type' content='text/html; charset=utf-8' />
       <meta charset='UTF-8'>
        <title>Mellombels resultat</title>
<style>
#customers
{
font-family:'Trebuchet MS', Arial, Helvetica, sans-serif;
width:100%;
border-collapse:collapse;
}
#customers td, #customers th 
{
font-size:1em;
border:1px solid #98bf21;
padding:3px 7px 2px 7px;
}
#customers th 
{
font-size:1.1em;
text-align:left;
padding-top:5px;
padding-bottom:4px;
background-color:#A7C942;
color:#ffffff;
}
#customers tr.alt td 
{
color:#000000;
background-color:#EAF2D3;
}

@media print
{
footer {page-break-after:always;}
}

</style>
</head>
<body>

<div style='padding-top:3.4cm;height:7cm;width:100%'>
	<div style='width:49%;float:left;'>
		<div style='padding-top:20px;'>Startnummer:</div>
		<div style='font-size:50pt'>
            #startnummer#
		</div>
	</div>
	<div style='width:49%;float:right;'>
		<div style='padding-top:40px;'>Brikkenummer:</div>
		<div style='font-size:50pt'>
            #brikke#
		</div>
	</div>
</div>

<hr style='margin-top:40px;margin-bottom:40px;border:0;border-bottom: 1px dashed #ccc;background:#999'/>

<div style='font-size:20pt;text-align:center;width:100%'>
	<h2>Mellombels resultat</h2>
";

    }
}
