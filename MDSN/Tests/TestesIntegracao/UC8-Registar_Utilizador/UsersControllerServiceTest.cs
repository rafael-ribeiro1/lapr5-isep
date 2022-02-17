using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using DDDSample1.Domain.Users;
using DDDSample1.Domain.Shared;
using DDDSample1.Controllers;
using Xunit;
using Moq;

namespace Tests.TestesIntegracao.UC8
{
    public class UsersControllerServiceTest
    {
        private readonly Mock<IUnitOfWork> _unit;
        private readonly Mock<IUserRepository> _repo;
        private readonly UserService _service;
        private readonly UsersController _controller;
        private CreatingUserDto cdto;
        private User usr;
        private UserDto rdto;

        public UsersControllerServiceTest()
        {
            _unit = new Mock<IUnitOfWork>();
            _repo = new Mock<IUserRepository>();
            _service = new UserService(_unit.Object, _repo.Object,null,null);
            _controller = new UsersController(_service, null);
            _unit.Setup(x => x.CommitAsync()).Returns(Task.FromResult(1));

            List<string> tags = new List<string>();
            tags.Add("A");
            cdto = new CreatingUserDto("Abc", "2000-10-10", "http://www.gravatar.com/avatar/a16a38cdfe8b2cbd38e8a56ab93238d3",
                "Porto", "Portugal", "abc@gmail.com", "Abcde123!", 
                "aaabbbccc", "+351987654321", tags, "https://www.facebook.com/id123", "https://www.linkedin.com/in/id123");
            List<Tag> rtags = new List<Tag>();
            rtags.Add(new Tag("A"));
            usr = new User("Abc", "2000/10/10", "Porto", "Portugal", "abc@gmail.com", "Abcde123!", 
                "aaabbbccc", "+351987654321", "https://www.linkedin.com/in/id123", "https://www.facebook.com/id123", tags);
            rdto = new UserDto(Guid.NewGuid(), "2000/10/10", "abc@gmail.com", tags);
        }

        [Fact]
        public async void ReturnBadRequestWhenDataIsInvalid()
        {
            CreatingUserDto dto = new CreatingUserDto("", "", "", "", "", "", "", "", "", null, "", "");
            var res = await _controller.PostUser(dto);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async void ReturnCreatedAtActionWhenUserIsRegisteredSuccessfully()
        {
            _repo.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.FromResult<User>(usr));
            var res = await _controller.PostUser(cdto);
            Assert.IsType<CreatedAtActionResult>(res);
        }

        [Fact]
        public async void ReturnExpectedObjectWhenUserIsRegisteredSuccessfully()
        {
            _repo.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.FromResult<User>(usr));
            var res = await _controller.PostUser(cdto);
            var action = res as CreatedAtActionResult;
            Assert.Equal(rdto, action.Value);
        }

        [Fact]
        public async void ReturnBadRequestWhenUserAlreadyExists()
        {
            _repo.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.FromResult<User>(usr));
            await _controller.PostUser(cdto);
            _repo.Setup(x => x.AddAsync(It.IsAny<DDDSample1.Domain.Users.User>())).Throws(new DbUpdateException());
            var res = await _controller.PostUser(cdto);
            Assert.IsType<BadRequestObjectResult>(res);
        }
    }
}