import tensorflow
from keras.models import load_model
import os
import pandas as pd
import numpy as np
from PIL import Image
import keras

def padding(array, xx, yy):
    """
    :param array: numpy array
    :param xx: desired height
    :param yy: desirex width
    :return: padded array
    """

    h = array.shape[0]
    w = array.shape[1]

    a = (xx - h) // 2
    aa = xx - a - h

    b = (yy - w) // 2
    bb = yy - b - w

    return np.pad(array, pad_width=((a, aa), (b, bb), (0, 0)), mode='constant')

def pooling(mat,ksize,method='max',pad=False):
    '''Non-overlapping pooling on 2D or 3D data.

    <mat>: ndarray, input array to pool.
    <ksize>: tuple of 2, kernel size in (ky, kx).
    <method>: str, 'max for max-pooling,
                   'mean' for mean-pooling.
    <pad>: bool, pad <mat> or not. If no pad, output has size
           n//f, n being <mat> size, f being kernel size.
           if pad, output has size ceil(n/f).

    Return <result>: pooled matrix.
    '''

    m, n = mat.shape[:2]
    ky,kx=ksize

    _ceil=lambda x,y: int(np.ceil(x/float(y)))

    if pad:
        ny=_ceil(m,ky)
        nx=_ceil(n,kx)
        size=(ny*ky, nx*kx)+mat.shape[2:]
        mat_pad=np.full(size,np.nan)
        mat_pad[:m,:n,...]=mat
    else:
        ny=m//ky
        nx=n//kx
        mat_pad=mat[:ny*ky, :nx*kx, ...]

    new_shape=(ny,ky,nx,kx)+mat.shape[2:]

    if method=='max':
        result=np.nanmax(mat_pad.reshape(new_shape),axis=(1,3))
    else:
        result=np.nanmean(mat_pad.reshape(new_shape),axis=(1,3))

    return result

if __name__ == "__main__":
    model = load_model('my_model_3.h5')
    path = './data'
    file = open("data.csv", 'w')
    file.write('object;target\n')
    imgs = os.listdir(path)
    for i in imgs:
        file.write(f'{i};3\n')
    file.close()
    df = pd.read_csv('data.csv', sep=';')
    y = []
    l = 0
    for i in range(0, df.shape[0]):
        img_name = df.loc[i, 'object']
        img_data = np.asarray(Image.open(path + '/' + img_name))
        new_img = padding(img_data, 4000, 5000)
        new_img = pooling(new_img, (5, 5))
        y.append(model.predict(new_img.reshape(1,800,1000,3)))
        print(img_name, ' DONE! ', y[-1])
    y = np.array(y)
    y = y.reshape(y.shape[0], 2)
    print(y.shape)
    predict = np.argmax(y, axis=1)
    df['target'] = predict
    df.to_csv('./data.csv', sep = ';')
    print(predict)
    file = open('res.txt','w')
    print(y)
    for i in range(len(y)):
        img_name = df.loc[i, 'object']
        file.write(str(i))
        file.write('|')
        file.write(path)
        file.write('/')
        file.write(img_name)
        file.write('|')
        if (df.loc[i, 'target'] == 1):
            file.write('T\n')
        else:
            file.write('F\n')
    file.close()