import telebot, os, shutil
from telebot import types
from config import *

bot = telebot.TeleBot(TOKEN)
files_list = os.listdir(path)


@bot.message_handler(commands=["start"])
def start(m, res=False, ctr=0):
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    item_yes = types.KeyboardButton('Yes')
    item_no = types.KeyboardButton('No')
    markup.add(item_yes, item_no)
    img = open(f'data/{files_list[ctr]}')
    bot.send_photo(m.from_user.id, img, )


@bot.message_handler(content_types=["text"])
def handle_text(message: types.Message, ctr):
    if message.text == 'Yes':
        shutil.copy(f'data/{files_list[ctr]}', f'data_sorted/{files_list[ctr]}')
    ctr += 1
    img = open(f'data/{files_list[ctr]}')
    bot.send_photo(message.from_user.id, img)


if __name__ == '__main__':
    bot.polling(none_stop=True, interval=0)
