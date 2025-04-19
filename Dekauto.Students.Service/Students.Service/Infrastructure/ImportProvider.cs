using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using GraphQL;
using GraphQL.Client.Abstractions;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ImportProvider : IImportProvider
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection importConfig;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsService studentsService;
        private readonly DekautoContext context;
        private readonly IGraphQLClient graphQLClient;

        public ImportProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, 
            DekautoContext context, IStudentsService studentsService, IGraphQLClient graphQLClient)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.studentsService = studentsService;
            this.importConfig = configuration.GetSection("Services").GetSection("Import").GetSection("GraphQLEndpoint");
            this.graphQLClient = graphQLClient;
        }

        private async Task<IEnumerable<StudentExportDto>> SendImportAsync(ImportFilesAdapter files)
        {
            string ldBase64 = await ConvertToBase64(files.ld);
            string contractBase64 = await ConvertToBase64(files.contract);
            string journalBase64 = await ConvertToBase64(files.journal);
            var mutation = new GraphQLRequest
            {
                Query = @"
                mutation ($ld: String!, $contract: String!, $journal: String!) {
                    importStudents(
                        ld: $ld,
                        contract: $contract,
                        journal: $journal
                    ) {
                        name
                        surname
                        patronymic
                        gender
                        birthdayDate           # DateOnly? → String
                        birthdayPlace
                        phoneNumber
                        email
                        gradeBook

                        # Паспортные данные
                        passportSerial
                        passportNumber
                        passportIssuancePlace
                        passportIssuanceDate   # DateOnly? → String
                        passportIssuanceCode
                        citizenship

                        # Адреса
                        addressRegistrationIndex
                        addressRegistrationOblKrayAvtobl
                        addressRegistrationDistrict
                        addressRegistrationType
                        addressRegistrationStreet
                        addressRegistrationCity
                        addressRegistrationHouse
                        addressRegistrationHousingType
                        addressRegistrationHousing
                        addressRegistrationApartment
                        addressResidentialIndex
                        addressResidentialOblKrayAvtobl
                        addressResidentialDistrict
                        addressResidentialType
                        addressResidentialStreet
                        addressResidentialCity
                        addressResidentialHouse
                        addressResidentialHousingType
                        addressResidentialHousing
                        addressResidentialApartment
                        livingInDormitory
                            
                          # Образование
                        enrollementOrderDate    # DateOnly? → String
                        enrollementOrderNum
                        groupName
                        educationReceived
                        educationReceivedSerial
                        educationReceivedNum
                        educationReceivedDate   # DateOnly? → String
                        ooName
                        ooAddress
                        educationReceivedEndYear   # short? → Int
                        bonusScores
                        militaryService
                        maritalStatus
                        education
                        educationForm
                        faculty
                        courseOfTraining
                        course
                        educationStartYear
                        educationFinishYear
                        educationTime
                        educationBase
                        educationRelationForm
                        educationRelationNum
                        educationRelationDate   # DateOnly? → String

                        # Экзамены (GIA)
                        giaExam1Name
                        giaExam1Score
                        giaExam1Note
                        giaExam2Name
                        giaExam2Score
                        giaExam2Note
                        giaExam3Name
                        giaExam3Score
                        giaExam3Note
                        giaExam4Name
                        giaExam4Score
                        giaExam4Note
                            
                    }
                }",
                Variables = new
                {
                    ld = ldBase64,
                    contract = contractBase64,
                    journal = journalBase64
                }
            };

            var response = await graphQLClient.SendMutationAsync<ImportStudentsResponse>(mutation);
            if (response.Errors != null)
            {
                var errorMessages = string.Join(", ", response.Errors.Select(e => e.Message));
                throw new Exception($"GraphQL ошибка: {errorMessages}");
            }

            if (response.Data == null)
                throw new Exception("Ответ от сервера пуст");

            return response.Data.importStudents.Select(s => new StudentExportDto
            {
                Name = s.Name,
                Surname = s.Surname,
                Patronymic = s.Patronymic,
                GroupName = s.GroupName,
                Gender = s.Gender,
                BirthdayDate = s.BirthdayDate,
                BirthdayPlace = s.BirthdayPlace,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                GradeBook = s.GradeBook,
                PassportSerial = s.PassportSerial,
                PassportNumber = s.PassportNumber,
                PassportIssuancePlace = s.PassportIssuancePlace,
                PassportIssuanceDate = s.PassportIssuanceDate,
                PassportIssuanceCode = s.PassportIssuanceCode,
                Citizenship = s.Citizenship,
                AddressRegistrationIndex = s.AddressRegistrationIndex,
                AddressRegistrationOblKrayAvtobl = s.AddressRegistrationOblKrayAvtobl,
                AddressRegistrationDistrict = s.AddressRegistrationDistrict,
                AddressRegistrationType = s.AddressRegistrationType,
                AddressRegistrationStreet = s.AddressRegistrationStreet,
                AddressRegistrationCity = s.AddressRegistrationCity,
                AddressRegistrationHouse = s.AddressRegistrationHouse,
                AddressRegistrationHousingType = s.AddressRegistrationHousingType,
                AddressRegistrationHousing = s.AddressRegistrationHousing,
                AddressRegistrationApartment = s.AddressRegistrationApartment,
                AddressResidentialIndex = s.AddressResidentialIndex,
                AddressResidentialOblKrayAvtobl = s.AddressResidentialOblKrayAvtobl,
                AddressResidentialDistrict = s.AddressResidentialDistrict,
                AddressResidentialType = s.AddressResidentialType,
                AddressResidentialStreet = s.AddressResidentialStreet,
                AddressResidentialCity = s.AddressResidentialCity,
                AddressResidentialHouse = s.AddressResidentialHouse,
                AddressResidentialHousingType = s.AddressResidentialHousingType,
                AddressResidentialHousing = s.AddressResidentialHousing,
                AddressResidentialApartment = s.AddressResidentialApartment,
                LivingInDormitory = s.LivingInDormitory,
                EnrollementOrderDate = s.EnrollementOrderDate,
                EnrollementOrderNum = s.EnrollementOrderNum,
                GiaExam1Name = s.GiaExam1Name,
                GiaExam1Score = s.GiaExam1Score,
                GiaExam1Note = s.GiaExam1Note,
                GiaExam2Name = s.GiaExam2Name,
                GiaExam2Score = s.GiaExam2Score,
                GiaExam2Note = s.GiaExam2Note,
                GiaExam3Name = s.GiaExam3Name,
                GiaExam3Score = s.GiaExam3Score,
                GiaExam3Note = s.GiaExam3Note,
                GiaExam4Name = s.GiaExam4Name,
                GiaExam4Score = s.GiaExam4Score,
                GiaExam4Note = s.GiaExam4Note,
                EducationReceived = s.EducationReceived,
                EducationReceivedSerial = s.EducationReceivedSerial,
                EducationReceivedNum = s.EducationReceivedNum,
                EducationReceivedDate = s.EducationReceivedDate,
                OOName = s.OOName,
                OOAddress = s.OOAddress,
                EducationReceivedEndYear = s.EducationReceivedEndYear,
                BonusScores = s.BonusScores,
                MilitaryService = s.MilitaryService,
                MaritalStatus = s.MaritalStatus,
                Education = s.Education,
                EducationForm = s.EducationForm,
                Faculty = s.Faculty,
                CourseOfTraining = s.CourseOfTraining,
                Course = s.Course,
                EducationStartYear = s.EducationStartYear,
                EducationFinishYear = s.EducationFinishYear,
                EducationTime = s.EducationTime,
                EducationBase = s.EducationBase,
                EducationRelationForm = s.EducationRelationForm,
                EducationRelationNum = s.EducationRelationNum,
                EducationRelationDate = s.EducationRelationDate
            });
        }
        public class ImportStudentsResponse
        {
            public List<StudentExportDto> importStudents { get; set; }
        }

        private async Task<string> ConvertToBase64(IFormFile file)
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public async Task ImportFilesAsync(ImportFilesAdapter files)
        {
                if (files == null) throw new ArgumentNullException(nameof(files));
                if (files.ld == null) throw new ArgumentNullException(nameof(files.ld));
                if (files.contract == null) throw new ArgumentNullException(nameof(files.contract));
                if (files.journal == null) throw new ArgumentNullException(nameof(files.journal));

                // Отправка запроса в сервис "Импорт" и получение готового массива
                var newStudents = await SendImportAsync(files);

                // Конвертация
                var processedNewStudents = await studentsService.FromExportDtosAsync(newStudents);

                // Добавление в БД
                foreach (var student in processedNewStudents)
                {
                    await context.Students.AddAsync(student);
                }

                await context.SaveChangesAsync();

        }
    }
}
