using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class CompanyControllerTests : AbstractBaseTest
    {
        private const string Gcp = "0000346";
        private readonly CompanyController _companyController = new CompanyController(new CompanyRepository());
        private readonly CompanyRepository _companyRepository = new CompanyRepository();

        [TestMethod]
        public void TestRoundtripCompanyRepository()
        {
            OnSetUp();
            var company = new CompanyBuilder().CreateCompany();
            _companyRepository.AddCompanies(new List<Company> {company});
            Assert.AreEqual(_companyRepository.GetCompany(company.Gcp).Name, company.Name);
        }

        [TestMethod]
        public void TestGetCompanyByGcp()
        {
            OnSetUp();
            var companyBuilder = new CompanyBuilder().SetGcp(Gcp);
            _companyRepository.AddCompanies(new List<Company> {companyBuilder.CreateCompany()});
            var result = _companyController.Get(Gcp);

            var correctCompany = companyBuilder.CreateCompany();
            Assert.IsTrue(CompaniesAreEqual(correctCompany, result.Company));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestGetNonExistentCompany()
        {
            OnSetUp();
            try
            {
                _companyController.Get(Gcp);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Gcp));
            }
        }

        [TestMethod]
        public void TestAddCompanies()
        {
            OnSetUp();
            var companyBuilder = new CompanyBuilder().SetGcp(Gcp);
            var addCompaniesRequest = companyBuilder.CreateAddCompaniesRequest();

            var response = _companyController.Post(addCompaniesRequest);
            var databaseCompany = _companyRepository.GetCompany(Gcp);
            var correctCompany = companyBuilder.CreateCompany();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(CompaniesAreEqual(new Company(databaseCompany), correctCompany));
        }

        private bool CompaniesAreEqual(Company a, Company b)
        {
            return a.Gcp == b.Gcp
                   && a.Name == b.Name
                   && a.Addr2 == b.Addr2
                   && a.Addr3 == b.Addr3
                   && a.Addr4 == b.Addr4
                   && a.PostalCode == b.PostalCode
                   && a.City == b.City
                   && a.Tel == b.Tel
                   && a.Mail == b.Mail;
        }
    }
}