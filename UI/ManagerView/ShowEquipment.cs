﻿using HealthcareSystem.Entity;
using HealthcareSystem.Entity.Enumerations;
using HealthcareSystem.Entity.RoomModel;
using HealthcareSystem.Entity.RoomModel.MoveEquipmentFiles;
using HealthcareSystem.Entity.RoomModel.RenovationFiles;
using HealthcareSystem.Entity.RoomModel.RoomFiles;
using HealthcareSystem.RoleControllers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HealthcareSystem.UI.ManagerView
{
    partial class ShowEquipment : Form
    {
        public RoomService roomService;
        public Room room;
        public ShowEquipment(Room room)
        {
            InitializeComponent();
            this.room = room;
            roomService = new RoomService(new MoveEquipmentRepository(), new RoomRepository(), new RenovationRepository());
            loadEquipment();
            loadComboBoxes();
        }
        public void loadComboBoxes() {
            List<RoomType> rooms = Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToList();
            roomTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            roomTypeComboBox.DataSource = rooms;

            List<EquipmentType> equipment = Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>().ToList();
            itemTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            itemTypeComboBox.DataSource = equipment;


            quantityComboBox.DisplayMember = "Text";
            quantityComboBox.ValueMember = "Value";
            quantityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new[] {
                new { Text = ">10", Value = ">10" },
                new { Text = "<10", Value = "<10" },
                new { Text = "0 to 10", Value = "1t10" },
                
            };
            quantityComboBox.DataSource = items;




        }
        public void loadEquipment() 
        { 
            equipmentListView.Items.Clear();   
            if (room != null) 
            {
                foreach (Equipment equipment in room.equipments)
                {
                    ListViewItem item = new ListViewItem(equipment._id.ToString());
                    item.SubItems.Add(equipment.item);
                    item.SubItems.Add(equipment.type.ToString());
                    item.SubItems.Add(equipment.quantity.ToString());
                    item.SubItems.Add(equipment.isDynamic.ToString());
                    item.SubItems.Add(room.name);
                    equipmentListView.Items.Add(item);
                }
                
            
            }
        }
        private void showEquipment_Load(object sender, EventArgs e)
        {

        }

        public void FillEquipmentListView(Tuple<Equipment, string> itemAndRoom) 
        {
            ListViewItem item = new ListViewItem(itemAndRoom.Item1._id.ToString());
            item.SubItems.Add(itemAndRoom.Item1.item);
            item.SubItems.Add(itemAndRoom.Item1.type.ToString());
            item.SubItems.Add(itemAndRoom.Item1.quantity.ToString());
            item.SubItems.Add(itemAndRoom.Item1.isDynamic.ToString());
            item.SubItems.Add(itemAndRoom.Item2);
            equipmentListView.Items.Add(item);

        }

        private void itemNameButton_Click(object sender, EventArgs e)
        {
            equipmentListView.Items.Clear();
            
            
                foreach (Tuple<Equipment, string> itemAndRoom in roomService.GetEquipmentByItemName(itemNameTextBox.Text)) {
                    FillEquipmentListView(itemAndRoom);           
                
            }
        }

        private void roomTypeButton_Click(object sender, EventArgs e)
        {
            equipmentListView.Items.Clear();         

            foreach (Tuple<Equipment, string> itemAndRoom in roomService.GetEquipmentByRoomType(roomTypeComboBox.Text))
            {
                FillEquipmentListView(itemAndRoom);

            }


        }

        private void itemTypeButton_Click(object sender, EventArgs e)
        {

            equipmentListView.Items.Clear();

            foreach (Tuple<Equipment, string> itemAndRoom in roomService.GetEquipmentByItemType(itemTypeComboBox.Text))
            {
                FillEquipmentListView(itemAndRoom);

            }

        }

        private void quantityButton_Click(object sender, EventArgs e)
        {
            equipmentListView.Items.Clear();
            

            foreach (Tuple<Equipment, string> itemAndRoom in roomService.GetEquipmentByQuantity(quantityComboBox.SelectedValue.ToString()))
            {
                FillEquipmentListView(itemAndRoom);

            }

        }
    }
}
