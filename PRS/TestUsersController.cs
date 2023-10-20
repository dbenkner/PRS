using PRS.Models;
using PRS.Data;
using PRS.Controllers;
using Xunit;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Mvc;

namespace PRS


{
    public class TestUsersController
    {
        public readonly UsersController ursCtrl;
        public TestUsersController()
        {
            ursCtrl = new UsersController(new PRSContext());
        }
        [Fact]
        public async void TestLogin()
        {
            var user = await ursCtrl.Login("hhill", "propane");
            Assert.IsType<ActionResult<User>?>(user);
        }
    }

}
