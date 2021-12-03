import telebot, os, shutil, logging, json
from telebot import types
from config import *
from flask import Flask, request
from github import Github
from git import Repo
from urllib.request import urlopen

g = Github(GIT_TOKEN)
repo = Repo("DigitalConcern/wildhack")

commit_message = '1'

files_list = os.listdir(path)

bot = telebot.TeleBot(BOT_TOKEN)

server = Flask(__name__)

logger = telebot.logger
logger.setLevel(logging.DEBUG)


@bot.message_handler(commands=["start"])
def start(m, res=False):
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    item_yes = types.KeyboardButton('Yes')
    item_no = types.KeyboardButton('No')
    item_so = types.KeyboardButton('So-So')
    markup.add(item_yes, item_no, item_so)
    img = open(f'data/{files_list[0]}', 'rb')
    bot.send_photo(m.chat.id, img, reply_markup=markup)


@bot.message_handler(content_types=["text"])
def handle_text(message: types.Message):
    if message.text == 'Yes':
        repo.index.add(f'data/{files_list[0]}')
        repo.index.commit(commit_message)
        origin = repo.remote('origin')
        origin.push()
        files_list.pop(0)
    if message.text == 'So-So':
        repo.index.add(f'data_so-so/{files_list[0]}')
        repo.index.commit(commit_message)
        origin = repo.remote('origin')
        origin.push()
        files_list.pop(0)
    if message.text == 'No':
        files_list.pop(0)
    img = open(f'data/{files_list[0]}', 'rb')
    bot.send_photo(message.from_user.id, img)


@server.route(f'/{BOT_TOKEN}', methods=['POST'])
def redirect_message():
    json_string = request.get_data().decode('utf-8')
    update = telebot.types.Update.de_json(json_string)
    bot.process_new_updates([update])
    return '!', 200


if __name__ == '__main__':
    bot.remove_webhook()
    bot.set_webhook(url=APP_URL)
    server.run(host='0.0.0.0', port=int(os.environ.get("PORT", 5000)))
