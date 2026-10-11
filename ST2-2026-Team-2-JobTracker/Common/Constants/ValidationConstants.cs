namespace ST2_2026_Team_2_JobTracker.Common.Constants;

public static class ValidationConstants
{
    public static class Company
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 150;

        public const int WebsiteMaxLength = 500;

        public const int LocationMaxLength = 150;
    }

    public static class JobOffer
    {
        public const int TitleMinLength = 3;
        public const int TitleMaxLength = 150;

        public const int DescriptionMinLength = 10;
        public const int DescriptionMaxLength = 10000;

        public const int LocationMaxLength = 150;

        public const double SalaryMinValue = 0;
        public const double SalaryMaxValue = 10000000;

        public const int SalaryPrecision = 12;
        public const int SalaryScale = 2;
    }

    public static class Application
    {
        public const int NotesMaxLength = 2000;
    }
}