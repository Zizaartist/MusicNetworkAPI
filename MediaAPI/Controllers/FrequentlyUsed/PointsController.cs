using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiClick.Models;
using ApiClick.Models.RegisterModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiClick.Controllers.FrequentlyUsed
{
    public class PointsController
    {
        ClickContext _context;
        const decimal pointsCoef = 0.05m;
        const decimal pointsMax = 0.3m; //30%

        public PointsController(ClickContext _context)
        {
            this._context = _context;
        }

        public decimal GetMaxPayment(decimal _userPoints, Order _order) 
        {
            var sumCost = _order.OrderDetails.Sum(e => e.Price * e.Count);
            var costInPoints = sumCost * pointsMax; //Пока статичные 30%
            if (_userPoints > costInPoints)
            {
                return costInPoints;
            }
            else 
            {
                return _userPoints;
            }
        }

        /// <summary>
        /// Выполняет изменение значения текущих баллов
        /// затем документирует изменение в виде регистра
        /// </summary>
        /// <returns>Успешность операции</returns>
        public bool StartTransaction(decimal _points, int? _senderId, int _receiverId, int _orderId, out PointRegister register) 
        {
            User sender = null;
            User receiver;
            Order order;
            bool hasSender = _senderId != null;
            register = null;

            try
            {
                sender = _context.Users.Find(_senderId);
                receiver = _context.Users.Find(_receiverId); //Просто подтверждаем факт наличия
                order = _context.Orders.Find(_orderId);
            }
            catch 
            {
                return false; //пользователь или заказ не найден
            }

            if (hasSender) //Отправитель - человек
            {
                if (sender.Points < _points)
                {
                    return false; //недостаточно средств для действия
                }

                //Изымаем средства от отправителя
                try
                {
                    sender.Points -= _points;
                    _context.SaveChanges();
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                //Документируем
                register = new PointRegister()
                {
                    TransactionCompleted = false,
                    SenderId = _senderId,
                    Sender = sender,
                    Receiver = receiver,
                    ReceiverId = _receiverId,
                    OrderId = _orderId,
                    Points = _points,
                    CreatedDate = DateTime.UtcNow
                };
                _context.PointRegisters.Add(register);
                _context.SaveChanges();
            }
            catch 
            {
                //Возврат средств
                if (hasSender) 
                {
                    sender.Points += _points;
                    _context.SaveChanges();
                }
                return false;
            }
            return true;
        }

        public bool CompleteTransaction(int _pointRegisterId)
        {
            User receiver;
            PointRegister pointRegister;
            try
            {
                pointRegister = _context.PointRegisters.Find(_pointRegisterId);
                receiver = _context.Users.Find(pointRegister.ReceiverId);
            }
            catch
            {
                return false;
            }

            //Совершаем изменение
            try
            {
                if (!pointRegister.TransactionCompleted)
                {
                    receiver.Points += pointRegister.Points;
                    pointRegister.TransactionCompleted = true;
                    _context.SaveChanges();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool CancelTransaction(int _pointRegisterId)
        {
            User sender;
            PointRegister pointRegister;
            try
            {
                pointRegister = _context.PointRegisters.Find(_pointRegisterId);
                sender = _context.Users.Find(pointRegister.SenderId);
            }
            catch
            {
                return false;
            }

            //Совершаем изменение
            try
            {
                sender.Points += pointRegister.Points;
                _context.PointRegisters.Remove(pointRegister);
                _context.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static decimal CalculateSum(Order _order)
        {
            if (!_order.OrderDetails.Any() || _order.OrderDetails.Any(e => e.Price == default || e.Count == default)) throw new Exception("Ошибка при вычислении кэшбэка");
            return _order.OrderDetails.Sum(e => e.Price * e.Count);
        }

        public decimal CalculateCashback(Order _order) 
        {
            var pointlessSum = CalculatePointless(_order);
            return pointlessSum * pointsCoef;
        }

        public decimal CalculatePointless(Order _order) 
        {
            var sum = CalculateSum(_order);
            try
            {
                var points = _order.PointRegisterId != null ? _context.PointRegisters.Find(_order.PointRegisterId).Points : 0;
                return sum - points;
            }
            catch (Exception _ex) 
            {
                throw _ex;
            }
        }

        ///// <summary>
        ///// Один из возможных исходов процесса выполнения заказа
        ///// передача баллов владельцу бренда, начисление клиенту баллов, если была произведена оплата другими средствами
        ///// </summary>
        //public void RemovePoints(Order order)
        //{
        //    //Начисляем баллы пользователя владельцу бренда
        //    var brandOwner = _context.Users.Find(order.BrandOwnerId);
        //    brandOwner.Points += order.PointRegister.Points;
        //    Transaction(order.PointRegister.Points, );

        //    var register = _context.PointRegisters.Find(order.PointRegisterId);
        //    register.TransactionCompleted = true;
        //    _context.SaveChanges();
        //}

        ///// <summary>
        ///// Вычисляет 5% от денежной суммы и переводит на счет пользователя баллами
        ///// </summary>
        //public void GetPoints(Order order, decimal points)
        //{
        //    decimal sum = order.OrderDetails.Sum(s => Convert.ToInt32(s.Price) * s.Count);
        //    decimal moneySum = sum - points;
        //    var user = _context.Users.Find(order.UserId);
        //    user.Points += Convert.ToInt32(moneySum * pointsCoef);
        //    _context.SaveChanges();
        //}

        ///// <summary>
        ///// Один из возможных исходов процесса выполнения заказа
        ///// возврат баллов клиенту
        ///// </summary>
        //public async Task ReturnPoints(PointRegister pointRegister)
        //{
        //    pointRegister.Owner.Points += pointRegister.Points;
        //    var register = await _context.PointRegisters.FindAsync(pointRegister.PointRegisterId);
        //    register.TransactionCompleted = true;
        //    await _context.SaveChangesAsync();
        //}

        ///// <summary>
        ///// Распределеяет баллы равномерно по всем предъявленным заказам
        ///// </summary>
        ///// <returns>Список числовых значений баллов для каждого заказа</returns>
        //public List<OrderExtended> DistributePoints(List<Order> orders)
        //{
        //    decimal currentPoints = _context.Users.Find(orders.First().UserId).Points;
        //    //Включает в себя те, заказы, которые оплачены полностью
        //    List<OrderExtended> notYetPaidOrders = new List<OrderExtended>();
        //    orders.ForEach(e => 
        //    {
        //        OrderExtended oe = new OrderExtended();
        //        oe.order = e;
        //        oe.orderSum = e.OrderDetails.Sum(x => x.Price * x.Count);
        //        oe.pointsInvested = 0;
        //        notYetPaidOrders.Add(oe);
        //    });

        //    List<OrderExtended> results = new List<OrderExtended>();
        //    bool fairlyDistributed = false;

        //    //АЛГОРИТМ РАСПРЕДЕЛЕНИЯ
        //    //Каждую итерацию должна производиться оплата минимум 1 заказа
        //    while (!fairlyDistributed) 
        //    {
        //        OrderExtended paidOrder;
        //        //Сперва получаем неточную "среднюю" величину
        //        decimal notPreciseEvenSum = GetAverage(currentPoints, notYetPaidOrders.Count);
        //        //Если она может покрыть хоть 1 заказ - оплатить и запустить следующую итерацию
        //        paidOrder = notYetPaidOrders.FirstOrDefault(e => e.orderSum <= notPreciseEvenSum);
        //        if (paidOrder == null)
        //        {
        //            //Оплачиваем среднюю величину для всех
        //            while (notYetPaidOrders.Count > 0)
        //            {
        //                paidOrder = notYetPaidOrders.First();
        //                notPreciseEvenSum = GetAverage(currentPoints, notYetPaidOrders.Count);
        //                DistributePoints_paidOrder(ref currentPoints, paidOrder, results, notYetPaidOrders, notPreciseEvenSum);
        //            }
        //        }
        //        else
        //        {
        //            //Оплачиваем полную стоимость заказа
        //            DistributePoints_paidOrder(ref currentPoints, paidOrder, results, notYetPaidOrders, paidOrder.orderSum);
        //        }
        //        if (notYetPaidOrders.Count == 0) 
        //        {
        //            fairlyDistributed = true;
        //        }
        //    }

        //    return results;
        //}

        //Действия при "оплате"

        //private void DistributePoints_paidOrder(ref decimal currentPoints, OrderExtended paidOrder, List<OrderExtended> results, List<OrderExtended> notYetPaidOrders, decimal points)
        //{
        //    currentPoints -= points;
        //    paidOrder.pointsInvested = points;
        //    results.Add(paidOrder);
        //    notYetPaidOrders.Remove(paidOrder);
        //}

        //private decimal GetAverage(decimal sumInput, int countInput) 
        //{
        //    return Math.Round(sumInput / countInput, 2);
        //}
    }
}
