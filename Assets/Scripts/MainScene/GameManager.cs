using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Slider sld;
    public TMP_Text dateText;
    public TMP_InputField dateInput;
    private Date[] sorted_Dates;
    private CubeColor[] allCubeColorScripts;
    private bool userModifiedInput = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Another instance of GameManager already exist");
            Destroy(gameObject);
            return;
        }
        this.userModifiedInput = false;
    }

    void Start()
    {
        this.sld.minValue = 0;
        this.sld.maxValue = 100;
        dateInput.onValueChanged.AddListener(OnDateInputChanged);
        dateInput.onSubmit.AddListener(OnDateInputSubmit);
        dateInput.onEndEdit.AddListener(OnDateInputEnd);
    }

    void OnDateInputChanged(string newText)
    {
        this.userModifiedInput = true;
    }

    void OnDateInputSubmit(string newText)
    {
        string textDate = this.dateInput.text;
        string searchDate = textDate.Split(' ')[0];
        string searchDay = searchDate.Split('/')[0];
        string searchMonth = searchDate.Split('/')[1];
        string searchYear = searchDate.Split('/')[2];
        string searchMorn = textDate.Split(' ')[1];

        int nb_Date = this.sorted_Dates.Length;
        float range = this.sld.maxValue / nb_Date;

        int i = 0;
        foreach (Date date in sorted_Dates)
        {
            int year = date.Year;

            int monthInt = date.Month;
            string month = monthInt < 10 ? "0" + monthInt.ToString() : monthInt.ToString();

            int dayInt = date.Day;
            string day = dayInt < 10 ? "0" + dayInt.ToString() : dayInt.ToString();

            string morn = date.Morning ? "AM" : "PM";

            if (searchDay == day && searchMonth == month && searchYear == year.ToString() && searchMorn == morn)
            {
                this.sld.value = i * range;
                break;
            }

            i += 1;
        }
        this.userModifiedInput = false;
    }

    void OnDateInputEnd(string newText)
    {
        this.userModifiedInput = false;
    }

    public void setup()
    {
        allCubeColorScripts = FindObjectsByType<CubeColor>(FindObjectsSortMode.None);
        List<Date> allDatesCombined = new List<Date>();

        foreach (CubeColor scriptInstance in allCubeColorScripts)
        {
            allDatesCombined.AddRange(scriptInstance.Dates);
        }

        List<Date> allUniqueDates = allDatesCombined
                                .GroupBy(d => new { d.Year, d.Month, d.Day, d.Morning })
                                .Select(g => g.First())
                                .ToList();

        Date[] allDateArray = allUniqueDates.ToArray();

        this.sorted_Dates = this.algo_sort_date(allDateArray);

        foreach (Date date in sorted_Dates)
        {
            Debug.Log($"{date.Year}/{date.Month}/{date.Day} {date.Morning}");
        }

        List<(Date, List<CubeColor>)> link = this.link_date_cube();
        this.manage_multiple(link);
    }

    public List<(Date, List<CubeColor>)> link_date_cube()
    {
        List<(Date, List<CubeColor>)> link = new List<(Date, List<CubeColor>)>();

        foreach (Date targetDate in sorted_Dates)
        {
            List<CubeColor> cube = new List<CubeColor>();
            foreach (CubeColor cubeColor in allCubeColorScripts)
            {
                bool hasDate = cubeColor.Dates.Any(d =>
                    d.Year == targetDate.Year &&
                    d.Month == targetDate.Month &&
                    d.Day == targetDate.Day &&
                    d.Morning == targetDate.Morning
                );

                if (hasDate)
                {
                    cube.Add(cubeColor);
                }
            }
            link.Add((targetDate, cube));
        }

        return link;
    }

    public void manage_multiple(List<(Date, List<CubeColor>)> link)
    {
        foreach ((Date targetDate, List<CubeColor> cubes) in link)
        {
            Dictionary<string, List<CubeColor>> analCodeGroups = new();

            foreach (CubeColor cubeInstance in cubes)
            {
                int indexAnalCode = cubeInstance.GetOccupIndexByDate(targetDate);
                if (indexAnalCode == -1) continue;

                string analCode = cubeInstance.Occupations[indexAnalCode].Item1;

                if (!analCodeGroups.ContainsKey(analCode))
                {
                    analCodeGroups[analCode] = new List<CubeColor>();
                }

                analCodeGroups[analCode].Add(cubeInstance);
            }

            foreach (var entry in analCodeGroups)
            {
                string analCode = entry.Key;
                List<CubeColor> associatedCubes = entry.Value;

                int count = associatedCubes.Count;

                if (count > 1)
                {
                    int indexOccupation = associatedCubes[0].GetOccupIndexByDate(targetDate);
                    int nb = associatedCubes[0].Occupations[indexOccupation].Item2;
                    int remains = nb % count;
                    foreach (CubeColor cube in associatedCubes)
                    {
                        int indexOcc = cube.GetOccupIndexByDate(targetDate);
                        if (remains == 0)
                        {
                            cube.SetNbOccupation(indexOcc, nb / count);
                        }
                        else
                        {
                            cube.SetNbOccupation(indexOcc, nb / count + 1);
                            remains -= 1;
                        }
                    }
                }
            }
        }
    }

    public void set_cube()
    {
        int nb_Date = this.sorted_Dates.Length;
        float range = this.sld.maxValue / nb_Date;

        for (int i = 0; i < nb_Date; i++)
        {
            if (sld.value >= i * range && sld.value < (i + 1) * range)
            {
                Date date = sorted_Dates[i];
                int year = date.Year;

                int dayInt = date.Day;
                string day = dayInt < 10 ? "0" + dayInt.ToString() : dayInt.ToString();
                string day_of_week = date.getDay();

                string month_of_year = date.getMonth();

                string morn_message = date.Morning ? "Matin" : "Apres-midi";

                this.dateText.SetText($"{day_of_week} {day} {month_of_year} {year} {morn_message}");

                if (!userModifiedInput)
                {
                    int monthInt = date.Month;
                    string month = monthInt < 10 ? "0" + monthInt.ToString() : monthInt.ToString();

                    string morn = date.Morning ? "AM" : "PM";

                    dateInput.onValueChanged.RemoveListener(OnDateInputChanged);
                    this.dateInput.text = $"{day}/{month}/{year} {morn}";
                    dateInput.onValueChanged.AddListener(OnDateInputChanged);
                }

                foreach (CubeColor scriptInstance in allCubeColorScripts)
                {
                    scriptInstance.setup_cube(this.sorted_Dates[i]);
                }
            }
        }
    }

    Date[] algo_sort_date(Date[] dates)
    {
        sort_by_year(ref dates);
        sort_by_month(ref dates);
        sort_by_day(ref dates);
        sort_by_half(ref dates);
        return dates;
    }

    void sort_by_year(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year > dates[j + 1].Year)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_month(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month > dates[j + 1].Month)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_day(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month == dates[j + 1].Month &&
                  dates[j].Day > dates[j + 1].Day)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_half(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month == dates[j + 1].Month &&
                  dates[j].Day == dates[j + 1].Day)
                {
                    if (!dates[j].Morning && dates[j + 1].Morning)
                    {
                        Date temp = dates[j];
                        dates[j] = dates[j + 1];
                        dates[j + 1] = temp;
                        break;
                    }
                }
            }
        }
    }
}
