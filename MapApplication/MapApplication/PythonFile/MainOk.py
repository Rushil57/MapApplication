from qgis.core import *
from qgis.PyQt.QtCore import *
# from PyQt5.QtCore import *
# from PyQt5 import QtCore

from qgis.core.additions.edit import edit
#from QdivideFunctions import *
#import callCleanup
import pandas as pd
import LoadData
import QIN

dfile=LoadData.getRawData('##ExcelPath##',0)

TN=[]
RN=[]
SECT=[]
PK=[]


failureCount = 0

for index,r in dfile.iterrows():
  TN.append(r["TOWNSHIP"])
  RN.append(r["RANGE"])
  SECT.append(r["SECTION"])
  PK.append(r["CGA_UNIT"])

print('TotalTracts: ',str(len(TN)))

QIN.buildPLSSLayer(TN,RN,SECT,PK)

print("complete")