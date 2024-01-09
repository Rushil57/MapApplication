var divMapEle = $('#divMap');
var externalSHPfileEle = $("#externalSHPfile");
var divMapStr = '<div id="map" class="map mapCls"><div id="info"></div></div>';
$('#btnLoad').click(function () {
    AddLoader();
    var shpFileVal = $("#txt_shp_file_name").val();
    if (shpFileVal === "") {
        alert("Please select SHP file to load");
        return;
    }
    $(".loader-parent").show();
    $('#btn_save_shpfile').prop('disabled', true);
    divMapEle.html('');
    $.ajax({
        type: "POST",
        url: '/Map/ShpToGeoJson',
        data: formData,
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {
            externalSHPfileEle.val('');
            $(".loader-parent").hide();
            var result = JSON.parse(response);
            //$(".loader-parent").hide();
            if (result.IsValid) {
                divMapEle.html('');
                divMapEle.html(divMapStr);
                alert("File load successfully");
                var geojsonObject = {};
                var isFirstTimeLoad = true;
                loadshp({
                    //url: '/Scripts/Delete/IN.zip', // path or your upload file
                    url: '/Files/' + shpFileVal.replace('.shp','.zip'), // path or your upload file
                    encoding: 'big5', // default utf-8
                    EPSG: 4326, // default 4326
                    async: false,
                }, function (geojson) {
                    if (!isFirstTimeLoad) {
                        return;
                    }
                    isFirstTimeLoad = false;
                    geojsonObject = geojson;
                    const styles = {
                        'LineString': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'green',
                                width: 1,
                            }),
                        }),
                        'MultiLineString': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'green',
                                width: 1,
                            }),
                        }),
                        'MultiPolygon': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'yellow',
                                width: 1,
                            }),
                            fill: new ol.style.Fill({
                                color: 'rgba(255, 255, 0, 0.1)',
                            }),
                        }),
                        'Polygon': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'blue',
                                lineDash: [4],
                                width: 3,
                            }),
                            fill: new ol.style.Fill({
                                color: 'rgba(0, 0, 255, 0.1)',
                            }),
                        }),
                        'GeometryCollection': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'magenta',
                                width: 2,
                            }),
                            fill: new ol.style.Fill({
                                color: 'magenta',
                            }),
                        }),
                        'Circle': new ol.style.Style({
                            stroke: new ol.style.Stroke({
                                color: 'red',
                                width: 2,
                            }),
                            fill: new ol.style.Fill({
                                color: 'rgba(255,0,0,0.2)',
                            }),
                        }),
                    };

                    const styleFunction = function (feature) {
                        return styles[feature.getGeometry().getType()];
                    };

                    const vectorSource = new ol.source.Vector({
                        features: new ol.format.GeoJSON().readFeatures(geojsonObject),
                    });

                    const vectorLayer = new ol.layer.Vector({
                        source: vectorSource,
                        style: styleFunction,
                    });

                    var map = new ol.Map({
                        target: 'map',
                        layers: [
                            new ol.layer.Tile({
                                source: new ol.source.OSM()
                            }),
                            vectorLayer
                        ],
                        view: new ol.View({
                            projection: 'EPSG:4326',
                            center: [-10909310.10, 4650301.84],
                            zoom: 5,
                            units: 'us-ft'
                        })
                    });
                    const info = document.getElementById('info');

                    let currentFeature;
                    const displayFeatureInfo = function (pixel, target) {
                        const feature = target.closest('.ol-control')
                            ? undefined
                            : map.forEachFeatureAtPixel(pixel, function (feature) {
                                return feature;
                            });
                        if (feature) {
                            info.style.left = pixel[0] + 'px';
                            info.style.top = pixel[1] + 100 + 'px';
                            let featureLength = feature.getKeys().length;
                            let innerInfoStr = '';
                            if (feature !== currentFeature && featureLength > 6) {
                                info.style.visibility = 'visible';
                                for (var i = 4; i <= featureLength; i++) {
                                    let currentFeature = feature.getKeys()[i - 1];
                                    innerInfoStr += currentFeature + ' = ' + feature.get(currentFeature) + "\n";
                                }
                                info.innerText = innerInfoStr;
                            }
                        } else {
                            info.style.visibility = 'hidden';
                        }
                        currentFeature = feature;
                    };

                    map.on('pointermove', function (evt) {
                        if (evt.dragging) {
                            info.style.visibility = 'hidden';
                            currentFeature = undefined;
                            return;
                        }
                        const pixel = map.getEventPixel(evt.originalEvent);
                        displayFeatureInfo(pixel, evt.originalEvent.target);
                    });

                    map.on('click', function (evt) {
                        displayFeatureInfo(evt.pixel, evt.originalEvent.target);
                    });

                    map.getTargetElement().addEventListener('pointerleave', function () {
                        currentFeature = undefined;
                        info.style.visibility = 'hidden';
                    });
                });

            }
            else {
                alert(result.Message);
            }
            $('#btn_save_shpfile').prop('disabled', false);

            RemoveLoader();
        },
        error: function (err1, err2, err3) {
            externalSHPfileEle.val('');
            $(".loader-parent").hide();
            $("#ModalSHP").modal('hide');

            RemoveLoader();
        }
    });
})


externalSHPfileEle.change(function () {
    var alertMsg = 'To import a shapefile you must select the .shp , .shx, .dbf, .cpg , .qmd and .prj files with the same name';
    var fileUpload = externalSHPfileEle.get(0);
    if (fileUpload != undefined) {
        var objFileLength = fileUpload.files.length;
        if (objFileLength < 4) {
            alert(alertMsg);
            return;
        }
        var filename = fileUpload.files[0].name;
        filename = filename.split('.')[0];
        var extension = "shp,shx,dbf,prj,cpg,qmd";
        for (var i = 0; i < fileUpload.files.length; i++) {
            var sfilename = fileUpload.files[i].name;
            if (filename == sfilename.split('.')[0]) {
                var matchExtension = extension.includes(sfilename.split('.')[1]);
                if (matchExtension == false) {
                    alert(alertMsg);
                    return;
                }
            }
            else {
                alert(alertMsg);
                return;
            }
        }
    }
    else {
        alert('Please choose a file');
        return;
    }
    UploadShpFile();
});


var formData
function UploadShpFile() {

    formData = new FormData();
    var fileUpload = externalSHPfileEle.get(0);
    if (fileUpload != undefined) {
        for (var i = 0; i < fileUpload.files.length; i++) {
            var sfilename = fileUpload.files[i].name;
            formData.append(sfilename, fileUpload.files[i]);
            if (sfilename.split('.')[1].toLowerCase() == "shp") {
                $("#txt_shp_file_name").val(sfilename);
            }
        }
    }
}