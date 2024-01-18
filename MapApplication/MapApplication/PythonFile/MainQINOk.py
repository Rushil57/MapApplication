from qgis.core import *
from qgis.PyQt.QtCore import *
from qgis.core.additions.edit import edit

def buildPLSSLayer(TN,RN,SECT,PK##mainOkFuncStr##): #,l_qq,MAPTID,qtrqtr):
  okLayer=QgsVectorLayer("##ShpVLayerPath##","OK Sections","ogr")

  for field in okLayer.fields():
    print(field.name(), field.typeName())

  uri = "MultiPolygon?crs=epsg:32025&field=id:integer"
  scratchLayer = QgsVectorLayer(uri, "PLSS_LAYER", "memory")
  pr=scratchLayer.dataProvider()
  pr.addAttributes([QgsField("PK", QVariant.String),
                    QgsField("SECTION", QVariant.String),QgsField("TOWNSHIP", QVariant.String),
                    QgsField("RANGE", QVariant.String)##mainQINQgsFieldStr##])
  scratchLayer.updateFields()

  f_indx=0
  recordCount = ##startTXCount##
  for s in SECT:
    okLayer.selectByExpression('"SECT_NUM"=\'' + str(s) + '\' and "DWM_TWN" = \'' + str(TN[f_indx]) + '\' and "DWM_RNG" = \'' + str(RN[f_indx]) + '\'')
    recordCount = recordCount + 1
    file = open("##myTxtProgressFile##","w")
    file.writelines(str(recordCount) + " / ##totalPer##")
    file.close()
    if len(okLayer.selectedFeatures())<1:
      print("Couldn't Find Section:",'"SECT_NUM"=\'' + str(s) + '\' and "DWM_TWN" = \'' + str(TN[f_indx]) + '\' and "DWM_RNG" = \'' + str(RN[f_indx]))
      f_indx=f_indx+1
      continue
    basePoly = okLayer.selectedFeatures()[0]
    if basePoly is None or basePoly.geometry() is None or basePoly.geometry().isNull():
      print("Hit Null value at i=" + str(f_indx))
      f_indx = f_indx + 1
      continue

    mpoly=basePoly.geometry()#.asMultiPolygon()
    if f_indx % 10 == 0:
        print(f_indx)


    fet=QgsFeature(scratchLayer.fields())
    fet.setGeometry(mpoly) #(QgsGeometry.fromMultiPolygonXY([[pts]]))
    fet.setAttributes([str(f_indx),str(PK[f_indx]),
                       str(SECT[f_indx]),
                       str(TN[f_indx]),str(RN[f_indx])##mainQINSetAttrStr##])
    (res,outf) =pr.addFeatures([fet])
    f_indx=f_indx+1
    
  scratchLayer.updateExtents()

  print("Creating File...")
  path="##shpGPath##"
  crs=QgsCoordinateReferenceSystem("epsg:32025")
  options = QgsVectorFileWriter.SaveVectorOptions()
  options.driverName = "ESRI Shapefile"
  QgsVectorFileWriter.writeAsVectorFormat(scratchLayer,path,'utf8',crs,"ESRI Shapefile")
  print("File Saved.  Process Complete")