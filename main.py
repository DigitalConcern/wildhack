import telebot, os, shutil, pathlib
from telebot import types
from config import *


files_list = os.listdir(path)
files_list_sorted = os.listdir('data_sorted/')
files_list_bad = os.listdir('data_bad/')
file = 0
while file != len(files_list):
    if files_list[file] in files_list_sorted:
        files_list.pop(file)
        file -= 1
    if files_list[file] in files_list_bad:
        files_list.pop(file)
        file -= 1
    file += 1

bot = telebot.TeleBot(BOT_TOKEN)


@bot.message_handler(commands=["start"])
def start(m, res=False):
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    item_yes = types.KeyboardButton('Yes')
    item_no = types.KeyboardButton('No')
    markup.add(item_yes, item_no)
    img = open(f'data/{files_list[0]}', 'rb')
    bot.send_photo(m.chat.id, img, reply_markup=markup)


@bot.message_handler(content_types=["text"])
def handle_text(message: types.Message):
    if not len(files_list):
        bot.send_message(message.from_user.id, 'Вы обработали все фотографии 😎')
    if message.text == 'Yes':
        shutil.move(f'data/{files_list[0]}', 'data_sorted/')
        files_list.pop(0)
    if message.text == 'No':
        shutil.copy(f'data/{files_list[0]}', 'data_bad/')
        files_list.pop(0)
    img = open(f'data/{files_list[0]}', 'rb')
    bot.send_photo(message.from_user.id, img)


if __name__ == '__main__':
    bot.polling(none_stop=True)
