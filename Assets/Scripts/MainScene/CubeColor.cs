using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem.Utilities;
using System.Linq;
using Unity.VisualScripting;

public class CubeColor : MonoBehaviour {
    private Color neutralRatioColor;
    private Color bordeauRatioColor;
    private Color greenRatioColor;
    private Color orangeRatioColor;
    private Color redRatioColor;
    private Color zeroRatioColor;
    private Color nonReservableColor;
    private Color bulleColor;
    public float ratio = -1.0f;
    private Dictionary<string, object> infos;

    public Dictionary<string, object> Infos {
        get { return infos; }
    }

    private List<(string, int)> occupations;
    public List<(string, int)> Occupations
    {
        get { return occupations; }
    }

    private List<Date> _dates;
    public List<Date> Dates {
        get { return _dates; }
    }

    public void SetNbOccupation(int indexOccupation, int n)
    {
        this.occupations[indexOccupation] = (this.occupations[indexOccupation].Item1, n);
    }

    void Awake()
    {
        this.occupations = new List<(string, int)>();
        this._dates = new List<Date>();

        this.infos = new Dictionary<string, object>();

        this.infos.Add("NomSalle", transform.name);
        this.infos.Add("Capacity", 1.0f);
        this.infos.Add("LibelleTypeSalle", "Mauvais Libelle");

        string csvInfosSalle = Path.Combine(Application.streamingAssetsPath, "InfosSalles.csv");
        string salleNomNormalise = Data.NormalizeString(transform.name);
        List<string[]> datas = Data.ReadCsvAndGetData(csvInfosSalle, salleNomNormalise, 0);

        if (datas.Count > 0) {
            if (datas[0][1].Trim() != "")
            {
                this.infos["Capacity"] = float.Parse(datas[0][1].Trim());
            }
            this.infos["LibelleTypeSalle"] = datas[0][2].Trim();
        }

        this.redRatioColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        this.orangeRatioColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        this.greenRatioColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        this.bordeauRatioColor = new Color(109f / 255f, 7.0f / 255f, 26.0f / 255.0f, 1.0f);
        this.zeroRatioColor = new Color(1.0f, 0.0f, 185.0f / 255.0f, 1.0f);
        this.neutralRatioColor = new Color(254.0f / 255.0f, 225.0f / 255.0f, 49.0f / 255.0f, 1.0f);
        this.nonReservableColor = new Color(48.0f / 255.0f, 48.0f / 255.0f, 48.0f / 255.0f, 1.0f);
        this.bulleColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        colorStatic();
    }

    void Update()
    {
        GameManager.Instance.set_cube();
        colorCube();
    }

    public void colorStatic()
    {
        string[] libelle = new string[] { "Bureau administratif", "Service informatique", "FABLAB", "Laboratoire electrique et numerique", "Laboratoire mecanique", "Salle de pause", "Salle de reunion", "Non reservable"};
        if (libelle.Contains(this.infos["LibelleTypeSalle"].ToString()))
        {
            GetComponent<Renderer>().material.color = nonReservableColor;
        }
        else if (this.infos["LibelleTypeSalle"].ToString() == "Bulle")
        {
            GetComponent<Renderer>().material.color = bulleColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = neutralRatioColor;
        }
    }

    public void colorCube()
    {
        string[] libelle = new string[] { "Bureau administratif", "Service informatique", "Bulle", "FABLAB", "Laboratoire electrique et numerique", "Laboratoire mecanique", "Salle de pause", "Salle de reunion", "Non reservable"};
        if (!libelle.Contains(this.infos["LibelleTypeSalle"].ToString()))
        {
            if (this.ratio == -1.0f)
            {
                GetComponent<Renderer>().material.color = neutralRatioColor;
            }
            else if (this.ratio > 1.0f)
            {
                GetComponent<Renderer>().material.color = bordeauRatioColor;
            }
            else if (this.ratio == 0.0f)
            {
                GetComponent<Renderer>().material.color = zeroRatioColor;
            }
            else if (this.ratio > 0.0f && this.ratio < 1.0f / 3.0f)
            {
                GetComponent<Renderer>().material.color = redRatioColor;
            }
            else if (this.ratio >= 1.0f / 3.0f && this.ratio < 2.0f / 3.0f)
            {
                GetComponent<Renderer>().material.color = orangeRatioColor;
            }
            else if (this.ratio >= 2.0f / 3.0f && this.ratio < 1.0f)
            {
                GetComponent<Renderer>().material.color = greenRatioColor;
            }
        }
    }

    public int GetOccupIndexByDate(Date search_date) {
        int index = -1;
        int i = 0;
        foreach (Date date in this._dates) {
            if (date.Year == search_date.Year && 
              date.Month == search_date.Month && 
              date.Day == search_date.Day && 
              date.Morning == search_date.Morning)
                {
                index = i;
                break;
            }
            i++;
        }

        return index;
    }

    public (float, string) setup_cube(Date search_date)
    {
        int index = -1;
        int i = 0;
        foreach (Date date in this._dates)
        {
            if (date.Year == search_date.Year &&
              date.Month == search_date.Month &&
              date.Day == search_date.Day &&
              date.Morning == search_date.Morning)
            {
                index = i;
                break;
            }
            i++;
        }
        if (index != -1)
        {
            this.ratio = occupations[index].Item2 / float.Parse(this.infos["Capacity"].ToString());
        }
        else
        {
            this.ratio = -1.0f;
        }
        return (this.ratio, this.infos["LibelleTypeSalle"].ToString());
    }

    public void onClick()
    {
        Debug.Log($"OnClick method for {transform.name}");

        if (occupations == null)
        {
            occupations = new List<(string, int)>();
        }

        if (_dates == null)
        {
            _dates = new List<Date>();
        }

        string csvFilePath = Path.Combine(Application.streamingAssetsPath, "ConvertedData.csv");
        List<string> XLColumns = Data.ReadCsvAndGetColumns(csvFilePath);

        int indexDate = XLColumns.IndexOf("Date");
        int indexHeure = XLColumns.IndexOf("Début");
        int indexSalle = XLColumns.IndexOf("NomSalle");
        int indexCodeAnal = XLColumns.IndexOf("CodeAnalytique");
        int indexNbInscrit = XLColumns.IndexOf("NombreInscrit");

        List<string[]> XLDatas = Data.ReadCsvAndGetData(csvFilePath, transform.name, indexSalle);

        for (int i = 0; i < XLDatas.Count; i++)
        {
            this.occupations.Add((XLDatas[i][indexCodeAnal].Trim(), int.Parse(XLDatas[i][indexNbInscrit].Trim())));
            string date_and_hour = XLDatas[i][indexDate].Trim();
            string date = date_and_hour.Split(' ')[0];
            int y = int.Parse(date.Split('/')[2]);
            int m = int.Parse(date.Split('/')[1]);
            int d = int.Parse(date.Split('/')[0]);
            bool morning = true;

            string time = XLDatas[i][indexHeure].Trim();

            int h = int.Parse(time.Split('H')[0]);

            if (h > 12) {
                morning = false;
            }

            this._dates.Add(new Date(y, m, d, morning));
        }
        
    }
}