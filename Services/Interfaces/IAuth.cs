using EBISX_POS.API.Services.DTO.Auth;

namespace EBISX_POS.API.Services.Interfaces
{
    public interface IAuth
    {
        Task<(bool success, bool isManager, string email, string name)> LogIn(LogInDTO logInDTO);
        Task<(bool, string)> LogOut(LogInDTO logOutDTO);
        Task<(bool, string, string)> HasPendingOrder();
        Task<(bool, string)> CheckData();
        Task<(bool, string)> LoadData();
        Task<(bool, string)> IsManagerValid(string managerEmail);
        Task<List<CashierDTO>> Cashiers();


        Task<(bool, string)> SetCashInDrawer(string cashierEmail, decimal cash);
        Task<(bool, string)> SetCashOutDrawer(string cashierEmail, decimal cash);
        Task<(bool, string)> CashWithdrawDrawer(string cashierEmail, string managerEmail, decimal cash);
        Task<bool> IsCashedDrawer(string cashierEmail);
        Task<bool> IsTrainMode();
        Task<bool> ChangeMode(string managerEmail);
    }
}
