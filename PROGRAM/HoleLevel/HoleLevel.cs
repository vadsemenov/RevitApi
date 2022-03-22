// Copyright Vadim Semenov(c) 2019 
//vad.s.semenov@gmail.com
//5587394@mail.ru
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;



namespace HoleLevel
{
    [Transaction(TransactionMode.Manual)]
    public class HoleLevel : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try {

                FilteredElementCollector collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_GenericModel);

                ElementType elemType;
                IList<Element> holeList = new List<Element>();
                
                
                //Собираем все проемы в документе
                foreach (Element elem in collector)
                {
                    elemType = doc.GetElement(elem.GetTypeId()) as ElementType;
                    if (elemType.FamilyName == "242_ISP_Проем_Цветной_(ОбщМод_Стена)_v02") {
                        holeList.Add(elem);
                    }   
                }

                double level = 0;
                //Вносим изменения в чертеж. Проставляем параметр отметки из свойства Location объекта проема.
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Проставляем отметки");
                    foreach (Element hole in holeList)
                    {
                        LocationPoint locationPoint = hole.Location as LocationPoint;
                        level = locationPoint.Point.Z; // 0.3048;
                        Parameter levelParam = hole.LookupParameter("О_ОтметкаОтЭтажа");
                        levelParam.Set(level);
                    }
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception exp) {
                message = exp.Message;
                return Result.Failed;
            }

        }
    }
}
