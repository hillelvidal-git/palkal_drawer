using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrawBL
{
    class WebAdapter
    {

        public bool ConnectServer(out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                client.TryIt();
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        public bool GetServerTime(out DateTime serverTime, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                serverTime = client.GetServerTime();
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                serverTime = new DateTime(2000, 1, 1);
                return false;
            }
        }

        public bool GetBox(int id, out object[] attributes, out double[][] samples, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfAnyType vals;
                BedekSurveyWebService.ArrayOfDouble[] samps;
                if (!client.GetSurvey(id, out vals, out samps, out msg))
                    throw new Exception(msg);
                attributes = vals.ToArray();
                samples = new double[samps.Length][];
                for (int i = 0; i < samps.Length; i++)
                    samples[i] = samps[i].ToArray();

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                attributes = new string[0];
                samples = new double[0][];
                return false;
            }
        }

        internal bool GetNewPoints(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news, out string msg)
        {
            string errorMsg = "";
            List<object[]> points = new List<object[]>();

            try
            {
                BedekSurveyWebService.ArrayOfInt ids;
                BedekSurveyWebService.ArrayOfAnyType s_new;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();

                //קבל את רשימת החדשות
                if (!client.GetNewPoints(levelId, lastDwgUpdate, out ids, out msg))
                    throw new Exception("WebService Error Getting IDs:\n" + msg);

                //עבור כל נקודה ברשימה, קבל את פרטיה מהשרת
                foreach (int id in ids)
                {
                    if (!client.GetPoint(id, out s_new))
                    {
                        errorMsg += id.ToString() + "; ";
                    }
                    points.Add(s_new.ToArray());
                }

                news = points.ToArray();

                if (errorMsg != "")
                {
                    msg = "These points failed to be downloaded: IDs " + errorMsg;
                    return false;
                }
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                news = new object[0][];
                return false;
            }
        }

        internal bool GetNewMeasurements(int levelId, int blocId, DateTime lastDwgUpdate, out object[][] news, out string msg)
        {
            string errorMsg = "";
            List<object[]> points = new List<object[]>();

            try
            {
                BedekSurveyWebService.ArrayOfInt ids;
                BedekSurveyWebService.ArrayOfAnyType s_new;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();

                //קבל את רשימת החדשות
                if (!client.GetNewMeasurements(levelId, lastDwgUpdate, out ids, out msg))
                    throw new Exception("WebService Error Getting IDs:\n" + msg);

                //עבור כל נקודה ברשימה, קבל את פרטיה מהשרת
                foreach (int id in ids)
                {
                    try
                    {
                        if (!client.GetMeasurement(id, out s_new))
                        {
                            errorMsg += id.ToString() + "; ";
                        }
                        points.Add(s_new.ToArray());
                    }
                    catch
                    {
                        errorMsg += id.ToString() + "; ";
                    }
                }

                news = points.ToArray();

                if (errorMsg != "")
                {
                    msg = "These points failed to be downloaded: IDs " + errorMsg;
                    return false;
                }
                msg = "";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                news = new object[0][];
                return false;
            }
        }

        internal bool GetNewBoxes(int levelId, int blocId, DateTime lastDwgUpdate, out int[] news, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfInt s_news;
                if (!client.GetNewSurveys(levelId, lastDwgUpdate, out s_news, out msg))
                    throw new Exception(msg);

                news = s_news.ToArray();
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                news = new int[0];
                return false;
            }
        }

        internal bool GetBorePosition(object[] ptVals, out double[] pos)
        {
            try
            {
                int attribution = (int)ptVals[0]; string boreName = (string)ptVals[1]; int ptClass = (int)ptVals[2];
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfDouble position;
                if (!client.GetBorePosition(attribution, boreName, ptClass, out position))
                    throw new Exception("WebMethod returned FALSE");

                pos = position.ToArray();
                return true;
            }
            catch
            {
                pos = new double[] { -1, -1, -1 };
                return false;
            }

        }

        internal bool GetBorePosition(object[] ptVals, out double[] pos, out string msg)
        {
            try
            {
                int levelId = (int)ptVals[0]; string boreName = (string)ptVals[1]; int ptClass = (int)ptVals[2];
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfDouble position;
                if (!client.GetGenPointPosition(levelId, boreName, out position, out msg))
                    throw new Exception("WebMethod returned FALSE: \n" + msg);

                pos = position.ToArray();
                return true;
            }
            catch (Exception e)
            {
                pos = new double[] { -1, -1, -1 };
                msg = e.Message;
                return false;
            }

        }

        internal bool GetProjectByLevel(int levelId, out int projectId, out string projectName)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetProjectByLevel(levelId, out projectId, out projectName))
                    throw new Exception("WebMethod retrned FALSE");
                return true;
            }
            catch
            {
                projectId = -1; projectName = "";
                return false;
            }

        }

        internal object[][] GetPointsDetails()
        {
            try
            {
                BedekSurveyWebService.ArrayOfAnyType[] details;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                details = client.GetPointsDetails();

                object[][] pDetails = new object[details.Length][];
                for (int i = 0; i < details.Length; i++)
                    pDetails[i] = details[i].ToArray();

                return pDetails;
            }
            catch
            {
                return new object[0][];
            }
        }

        internal bool GetLevelBlocs(int levelId, out object[][] blocs)
        {
            try
            {
                BedekSurveyWebService.ArrayOfAnyType[] s_blocs;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetLevelBlocs(levelId, out s_blocs)) throw new Exception("WebMethod returned false");

                blocs = new object[s_blocs.Length][];
                for (int i = 0; i < s_blocs.Length; i++)
                    blocs[i] = s_blocs[i].ToArray();

                return true;
            }
            catch
            {
                blocs = new object[0][];
                return false;
            }
        }

        internal bool GetLevelName(int levelId, out string levelName)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetLevelName(levelId, out levelName)) throw new Exception("WebMethod returned false");

                return true;
            }
            catch
            {
                levelName = "";
                return false;
            }
        }

        internal bool GetLineId(int intName, int fieldId, out int lineId, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetLineId(intName, fieldId, out lineId, out msg))
                    if (msg == "Line Not Found")
                    {
                        //Try to insert new line
                        if (!client.InsertNewLine(intName, fieldId, out lineId, out msg))
                            throw new Exception("Insert WebMethod returned false: " + msg);
                    }
                    else
                        throw new Exception("Select WebMethod returned false: " + msg);

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                lineId = -1;
                return false;
            }
        }


        internal bool GetFieldId(string name, int blocId, out int fieldId, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetFieldId(name, blocId, out fieldId, out msg))
                {
                    if (msg.Contains("Field Not Found"))
                    {
                        //Try to insert new line
                        if (!client.InsertNewField(name, blocId, out fieldId, out msg))
                            throw new Exception("Insert WebMethod returned false: " + msg);
                    }
                    else
                        throw new Exception("Select WebMethod returned false: " + msg);
                }
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                fieldId = -1;
                return false;
            }
        }


        internal bool UploadNewPoint(int ptNum, int lineId, double[] position, double rotation_Rad, int ptDetail, int statusId, int problem, out string msg, out int newId)
        {
            try
            {
                BedekSurveyWebService.ArrayOfDouble pos = new BedekSurveyWebService.ArrayOfDouble() { position[0], position[1], position[2] };
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.InsertNewPoint(ptNum, lineId, pos, rotation_Rad, ptDetail, statusId, problem, out msg, out newId))
                    throw new Exception("WebMethod returned false: \n" + msg);
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                newId = -1;
                return false;
            }

        }

        internal bool UpdatePointStatus(int dbId, int statusId, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.UpdatePointStatus(dbId, statusId, out msg))
                    throw new Exception("WebMethod returned false: " + msg);

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdatePointDetail(int dbId, int detailId, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.UpdatePointDetailId(dbId, detailId, out msg))
                    throw new Exception("WebMethod returned false: " + msg);

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdateReportValue(int repId, string column, object val, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.UpdateReportValue(repId, column, val, out msg))
                    throw new Exception("WebMethod returned false: " + msg);

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool GetReportValues(int ptId, out object[] repVals, out int ptStatusId, out string msg)
        {
            try
            {
                BedekSurveyWebService.ArrayOfAnyType s_vals;
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetReportValues(ptId, out s_vals, out ptStatusId, out msg))
                    throw new Exception("WebMethod returned false: " + msg);

                repVals = s_vals.ToArray();
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                repVals = new object[0];
                ptStatusId = -1;
                return false;
            }
        }

        internal bool GetPointReportID(int ptId, out int repId)
        {
            string msg = "";
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.GetPointReportID(out repId, ptId))
                    if (!client.CreateNewReport(out repId, ptId))
                        throw new Exception("WebMethod returned false: " + msg);
                return true;
            }
            catch (Exception e)
            {
                repId = -1;
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdatePointPosition(int ptId, double[] pos, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfDouble posArray = new BedekSurveyWebService.ArrayOfDouble();
                foreach (double p in pos) posArray.Add(p);
                if (!client.UpdatePointPosition(ptId, posArray, out msg))
                    throw new Exception("WebMethod returned false: " + msg);
                msg += "\nPosition: " + posArray[0] + ", " + posArray[1] + ", " + posArray[2];
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        internal bool UpdateSurveySamples(int boxDbId, List<double[]> vertices, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                BedekSurveyWebService.ArrayOfDouble[] v_array = new BedekSurveyWebService.ArrayOfDouble[vertices.Count];
                for (int i = 0; i < vertices.Count; i++) v_array[i] = new BedekSurveyWebService.ArrayOfDouble() { vertices[i][0], vertices[i][1] };

                //שליחת הנתונים לעדכון
                if (!client.ReplaceSurveySamples(boxDbId, v_array, out msg))
                    throw new Exception("WebMethod returned false: " + msg);
                return true;
            }
            catch (Exception e)
            {
                msg = "Connector error: " + e.Message;
                return false;
            }
        }

        internal bool UpdateReportValues(object[] PtIds, List<object[]> valsToUpdate, out int updated, out string msg)
        {
            try
            {
                BedekSurveyWebService.ArrayOfAnyType[] pairs=new BedekSurveyWebService.ArrayOfAnyType[valsToUpdate.Count];
                BedekSurveyWebService.ArrayOfAnyType ids=new BedekSurveyWebService.ArrayOfAnyType();
                foreach(object id in PtIds) ids.Add(id);
                for (int i = 0; i < valsToUpdate.Count; i++) pairs[i] = new BedekSurveyWebService.ArrayOfAnyType { valsToUpdate[i][0], valsToUpdate[i][1] };
                
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.UpdateReportValues(ids, pairs, out updated, out msg))
                    throw new Exception("WebMethod returned false: " + msg);

                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                updated = 0;
                return false;
            }
        }

        internal bool RotateLevel(int levelId, double RadAngle, out string msg)
        {
            try
            {
                BedekSurveyWebService.SurveyServiceSoapClient client = new BedekSurveyWebService.SurveyServiceSoapClient();
                if (!client.RotateLevel(levelId, RadAngle, out msg))
                        throw new Exception("WebMethod <RotateLevel> returned false: " + msg);
                msg += "\nSent Angle: " + RadAngle.ToString();
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }
    }
}
