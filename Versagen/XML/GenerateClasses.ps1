 param (
    [switch]$clean = $false
 )
#$script:fileList = dir -Recurse -Filter "*.xsd" .\Schemas
#if (!$clean){
#    $checkList = dir -Recurse -Filter "*.cs" | Select-Object -ExpandProperty BaseName
#    $fileList = $fileList | ?{$checkList -notcontains $_.BaseName }
#}else{
#    Remove-Item ".\SchemaClasses\*" -Filter "*.cs"
#}
#New-Item .\Schemas\Processed -ItemType directory
#$fileList | ForEach {
#    echo $_.BaseName ;
#    echo ".\Schemas\Processed\$_"
#    .\xsdPreprocessor.exe $_.FullName ".\Schemas\Processed\$_"

#}
#$script:processedList = $fileList | -select Path, Name | ForEach { return ".\Schemas\Processed\$_" }
#$processedList | ForEach {
#    echo ".\Schemas$_" ;
#    xsd /c "$_" /o:.\SchemaClasses /p:GenerateOptions.xml ;
#}
#Remove-Item .\Schemas\Processed
New-Item .\Schemas\Processed -ItemType directory
.\xsdPreprocessor.exe  ".\Schemas\Story.xsd" ".\Schemas\Processed\Story.xsd"
xsd /c ".\Schemas\Processed\Story.xsd" /o:.\SchemaClasses /p:GenerateOptions.xml ;
Remove-Item .\Schemas\Processed -Recurse