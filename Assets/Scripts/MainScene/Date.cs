using System;

public class Date
{
    private int _year;
    public int Year
    {
        get { return _year; }
        set { _year = value; }
    }
    private int _month;
    public int Month
    {
        get { return _month; }
        set { _month = value; }
    }
    private int _day;
    public int Day
    {
        get { return _day; }
        set { _day = value; }
    }
    private bool morning;
    public bool Morning
    {
        get { return morning; }
        set { morning = value; }
    }

    public Date(int year, int month, int day, bool morning)
    {
        this.Year = year;
        this.Month = month;
        this.Day = day;
        this.Morning = morning;
    }

    public DateTime ToDateTime()
    {
        return new DateTime(Year, Month, Day);
    }

    public string getDay()
    {
        DateTime date = ToDateTime();
        string day = date.DayOfWeek.ToString() switch
        {
            "Monday" => "Lundi",
            "Tuesday" => "Mardi",
            "Wednesday" => "Mercredi",
            "Thursday" => "Jeudi",
            "Friday" => "Vendredi",
            "Saturday" => "Samedi",
            "Sunday" => "Dimanche",
            _ => "Jour inconnu",
        };
        return day;
    }

    public string getMonth()
    {
        string month = Month switch
        {
            1 => "Janvier",
            2 => "Février",
            3 => "Mars",
            4 => "Avril",
            5 => "Mai",
            6 => "Juin",
            7 => "Juillet",
            8 => "Août",
            9 => "Septembre",
            10 => "Octobre",
            11 => "Novembre",
            12 => "Décembre",
            _ => "Mois inconnu",
        };

        return month;
    }
}
