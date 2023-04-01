//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Description: Тестирование.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Testing
{
    using RmSolution.Data;
    using RmSolution.Web;

    [TestClass]
    public class HttpClientTest
    {
        RmHttpClient _client = new();

        async Task TestData<T>(string id)
        {
            var resp = await _client?.QueryAsync(id);
            Assert.IsNotNull(resp);
            Assert.IsTrue(((IEnumerable<object?>)resp).Any());
            Assert.IsTrue(((IEnumerable<object?>)resp).FirstOrDefault() is T);
        }

        [TestMethod]
        public async Task GetURolesData()
        {
            await TestData<TRole>(WellKnownObjects.Role);
        }

        [TestMethod]
        public async Task GetUsersData()
        {
            await TestData<TUser>(WellKnownObjects.User);
        }

        [TestMethod]
        public async Task GetEquipmentTypesData()
        {
            await TestData<TEquipmentType>(WellKnownObjects.EquipmentType);
        }

        [TestMethod]
        public async Task GetEquipmentsData()
        {
            await TestData<TEquipment>(WellKnownObjects.Equipment);
        }
    }
}