using OrgStructModels.Persistables;

namespace OrgStructModels.Protocol
{
    public class PersonResult : AResultBase
    {
        public PersonResult() : base()
        {
            Person = new PersonModel();
        }

        public PersonResult(PersonModel person) : base ()
        {
            Person = person;
        }

        PersonModel Person { set; get; }

        public PersonResult(bool success, PersonModel person)
        {
            Success = true;
            Person = person;
        }
    }
}
