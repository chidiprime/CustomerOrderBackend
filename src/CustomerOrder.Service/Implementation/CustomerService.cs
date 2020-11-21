using CustomerOrder.Core.Domain;
using CustomerOrder.Core.Interface;
using CustomerOrder.Core.Utility;
using CustomerOrder.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using static CustomerOrder.Core.Utility.Enums;

namespace CustomerOrder.Service.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseModel<CustomerDTO>> Create(CustomerDTO customerDTO)
        {
            try
            {
                //Map customer view model to customer entity model
                var customer = new Customer
                {
                    Name=customerDTO.Name,
                    Age= Convert.ToDateTime(customerDTO.Age),
                    Gender=customerDTO.Gender                 
                };
                 _unitOfWork.CustomerRepository.Add(customer);

                foreach(var addressObj in customerDTO.Addresses)
                {
                    //Map address view model to address entity model
                    var address = new Address
                    {
                        Street = addressObj.Street,
                        PostCode = addressObj.PostCode,
                        HouseNumber = addressObj.HouseNumber,
                        CustomerID = customer.Id
                    };
                    _unitOfWork.AddressRepository.Add(address);
                }
                //commiting the transaction 
                await _unitOfWork.CompleteAsync();
                return new ResponseModel<CustomerDTO>
                { ResponseCode = Constants.OK, ResponseMessage = Constants.OK, ResponseData = customerDTO };

            }
            catch (Exception)
            {
                return new ResponseModel<CustomerDTO>
                { ResponseCode = Constants.Server_Error, ResponseMessage = Constants.Server_Error, ResponseData = null };
            }
        }

        public async Task<ResponseModel<bool>> Delete(int customerID)
        {
            try
            {
                //Get customer record
                var customer = _unitOfWork.CustomerRepository.SingleOrDefault(x => x.Id == customerID);
                if (customer == null)
                    return new ResponseModel<bool>
                    { ResponseCode = Constants.NOT_FOUND, ResponseMessage = Constants.CUSTOMER_NOT_FOUND, ResponseData = false };
               
                //Remove customer record
                _unitOfWork.CustomerRepository.Remove(customer);
                await _unitOfWork.CompleteAsync();  
                
                 return new ResponseModel<bool>
                 { ResponseCode = Constants.OK, ResponseMessage = Constants.OK, ResponseData = true };
            }
            catch (Exception)
            {

                return new ResponseModel<bool>
                { ResponseCode = Constants.Server_Error, ResponseMessage = Constants.Server_Error, ResponseData = false };
            }
        }

        public ResponseModel<List<CustomerDTO>> Search(string name)
        {
            try
            {
                var customerobj = _unitOfWork.CustomerRepository.Any(x => x.Name.ToLower() == name.ToLower());
                if(!customerobj)
                    return new ResponseModel<List<CustomerDTO>>
                    { ResponseCode = Constants.NOT_FOUND, ResponseMessage = Constants.CUSTOMER_NOT_FOUND, ResponseData = null };
                //Get customer record
                List <CustomerDTO> customer = _unitOfWork.CustomerRepository.Get(x => x.Name.ToLower() ==name.ToLower(),includeProperties:"Addresses,Orders").
                       Select(x=> new CustomerDTO {
                        Id=x.Id,
                        Name=x.Name,
                        Age=Convert.ToDateTime(x.Age).ToString(),
                        Gender=x.Gender,
                        Addresses =x.Addresses.
                        Select(a=>new AddressDTO {AddressId=a.Id,Street=a.Street,PostCode=a.PostCode,HouseNumber=a.HouseNumber }).ToList(),
                        Orders=x.Orders.
                        Select(a=>new OrderDTO {Amount=a.Amount, OrderDate=Convert.ToDateTime(a.OrderDate).ToString()}).ToList()
                        }).ToList();
                
                if (customer.Count==0)
                    return new ResponseModel<List<CustomerDTO>>
                    { ResponseCode = Constants.NOT_FOUND, ResponseMessage = Constants.CUSTOMER_NOT_FOUND, ResponseData = null };
              
                 return new ResponseModel<List<CustomerDTO>>
                 { ResponseCode = Constants.OK, ResponseMessage = Constants.OK, ResponseData = customer };

            }
            catch (Exception)
            {
                return new ResponseModel<List<CustomerDTO>>
                { ResponseCode = Constants.NOT_FOUND, ResponseMessage = Constants.CUSTOMER_NOT_FOUND, ResponseData = null };
            }
        }

        public async Task<ResponseModel<CustomerDTO>> Update(CustomerDTO customerDTO)
        {
            try
            {
                //Get customer record
                var customer = _unitOfWork.CustomerRepository.SingleOrDefault(x => x.Id == customerDTO.Id);
                if (customer == null)
                    return new ResponseModel<CustomerDTO>
                    { ResponseCode = Constants.NOT_FOUND, ResponseMessage = Constants.CUSTOMER_NOT_FOUND, ResponseData = null };
                customer.Name = customerDTO.Name;
                customer.Gender = customerDTO.Gender;
                customer.Age = Convert.ToDateTime(customerDTO.Age);
                foreach(var addressObj in customerDTO.Addresses)
                {
                    var address = _unitOfWork.AddressRepository.Get(addressObj.AddressId);
                    if(address != null)
                    {
                        address.Street = addressObj.Street;
                        address.PostCode = addressObj.PostCode;
                        address.HouseNumber = addressObj.HouseNumber;
                        _unitOfWork.AddressRepository.UpdateAsync(address);
                    }
                }
                //Remove customer record
                _unitOfWork.CustomerRepository.UpdateAsync(customer);
                await _unitOfWork.CompleteAsync();

                return new ResponseModel<CustomerDTO>
                { ResponseCode = Constants.OK, ResponseMessage = Constants.OK, ResponseData = customerDTO };
            }
            catch (Exception)
            {

                return new ResponseModel<CustomerDTO>
                { ResponseCode = Constants.Server_Error, ResponseMessage = Constants.Server_Error, ResponseData = null };
            }
        }
    }
}
