import telebot, os, shutil
from telebot import types
from config import *
from flask import Flask, request
import logging

bot = telebot.TeleBot(TOKEN)
files_list = os.listdir(path)
server = Flask(__name__)
logger = telebot.logger
logger.setLevel(logging.DEBUG)


@bot.message_handler(commands=["start"])
def start(m, res=False, ctr=0):
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    item_yes = types.KeyboardButton('Yes')
    item_no = types.KeyboardButton('No')
    markup.add(item_yes, item_no)
    img = open(f'data/{files_list[ctr]}', 'rb')
    bot.send_photo(m.chat.id, img, reply_markup=markup)


@bot.message_handler(content_types=["text"])
def handle_text(message: types.Message, ctr):
    if message.text == 'Yes':
        shutil.copy(f'data/{files_list[ctr]}', f'data_sorted/{files_list[ctr]}')
    ctr += 1
    img = open(f'data/{files_list[ctr]}', 'rb')
    bot.send_photo(message.from_user.id, img)


@server.route(f'/{TOKEN}', methods=['POST'])
def redirect_message():
    json_string = request.get_data().decode('utf-8')
    update = telebot.types.Update.de_json(json_string)
    bot.process_new_updates([update])
    return '!', 200


if __name__ == '__main__':
    bot.remove_webhook()
    bot.set_webhook(url=APP_URL)
    server.run(host='0.0.0.0', port=int(os.environ.get("PORT", 5000)))
