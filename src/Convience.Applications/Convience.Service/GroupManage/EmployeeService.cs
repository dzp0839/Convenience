﻿using Convience.Entity.Data;
using Convience.EntityFrameWork.Repositories;
using Convience.Jwtauthentication;
using Convience.Model.Models.GroupManage;
using Convience.Repository;
using Convience.Util.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Convience.Service.GroupManage
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUserRepository _userRepository;

        private readonly IUnitOfWork<SystemIdentityDbContext> _unitOfWork;

        public EmployeeService(IUserRepository userRepository,
            IUnitOfWork<SystemIdentityDbContext> unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public EmployeeResult GetEmployeeById(int id)
        {
            var users = from u in _userRepository.GetUsers()
                        let pquery = from uc in _userRepository.GetUserClaims()
                                     where u.Id == uc.UserId && uc.ClaimType == CustomClaimTypes.UserPosition
                                     select uc.ClaimValue
                        let dquery = from uc in _userRepository.GetUserClaims()
                                     where u.Id == uc.UserId && uc.ClaimType == CustomClaimTypes.UserDepartment
                                     select uc.ClaimValue
                        where u.Id == id
                        orderby u.Id descending
                        select new EmployeeResult
                        {
                            Id = u.Id,
                            PhoneNumber = u.PhoneNumber,
                            Avatar = u.Avatar,
                            Name = u.Name,
                            Sex = (int)u.Sex,
                            DepartmentId = dquery.FirstOrDefault(),
                            PositionIds = string.Join(',', pquery)
                        };
            return users.FirstOrDefault();
        }

        public (IEnumerable<EmployeeResult>, int) GetEmployees(EmployeeQuery query)
        {
            var where = ExpressionExtension.TrueExpression<SystemUser>()
                .AndIfHaveValue(query.Name, u => u.Name.Contains(query.Name))
                .AndIfHaveValue(query.PhoneNumber, u => u.PhoneNumber.Contains(query.PhoneNumber));
            var userQuery = _userRepository.GetUsers().Where(@where);

            if (query.DepartmentId != null)
            {
                userQuery = from u in userQuery
                            join uc in _userRepository.GetUserClaims() on u.Id equals uc.UserId
                            where uc.ClaimType == CustomClaimTypes.UserDepartment &&
                                    uc.ClaimValue == query.DepartmentId.ToString()
                            select u;
            }

            if (query.PositionId != null)
            {
                userQuery = from u in userQuery
                            join uc in _userRepository.GetUserClaims() on u.Id equals uc.UserId
                            where uc.ClaimType == CustomClaimTypes.UserPosition &&
                                    uc.ClaimValue == query.PositionId.ToString()
                            select u;
            }

            var users = from u in userQuery
                        let pquery = from uc in _userRepository.GetUserClaims()
                                     where u.Id == uc.UserId && uc.ClaimType == CustomClaimTypes.UserPosition
                                     select uc.ClaimValue
                        let dquery = from uc in _userRepository.GetUserClaims()
                                     where u.Id == uc.UserId && uc.ClaimType == CustomClaimTypes.UserDepartment
                                     select uc.ClaimValue
                        orderby u.Id descending
                        select new EmployeeResult
                        {
                            Id = u.Id,
                            PhoneNumber = u.PhoneNumber,
                            Avatar = u.Avatar,
                            Name = u.Name,
                            Sex = (int)u.Sex,
                            DepartmentId = dquery.FirstOrDefault(),
                            PositionIds = string.Join(',', pquery)
                        };
            var skip = query.Size * (query.Page - 1);
            return (users.Skip(skip).Take(query.Size), users.Count());
        }

        public async Task<bool> UpdateEmplyeeAsync(EmployeeViewModel viewModel)
        {
            var user = await _userRepository.GetUserByIdAsync(viewModel.Id.ToString());
            if (user != null)
            {
                await _unitOfWork.StartTransactionAsync();
                try
                {
                    await _userRepository.UpdateUserClaimsAsync(user, CustomClaimTypes.UserDepartment,
                        new List<string> { viewModel.DepartmentId });

                    await _userRepository.UpdateUserClaimsAsync(user, CustomClaimTypes.UserPosition,
                        viewModel.PositionIds.Split(',', StringSplitOptions.RemoveEmptyEntries));

                    await _unitOfWork.CommitAsync();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollBackAsync();
                    return false;
                }
            }
            return true;
        }
    }
}