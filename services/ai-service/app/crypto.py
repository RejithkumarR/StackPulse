# utils/crypto.py
import base64
import hashlib
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad

VECTOR = "RejithDev32123#423%43"


class AESCrypto:

    def __init__(self):
        # SHA256 key (same as C#)
        self.key = hashlib.sha256(VECTOR.encode("ascii")).digest()

        # 16-byte zero IV (same as C#)
        self.iv = bytes([0] * 16)

    def encrypt(self, plain_text: str) -> str:
        cipher = AES.new(self.key, AES.MODE_CBC, self.iv)

        encrypted = cipher.encrypt(
            pad(plain_text.encode("ascii"), AES.block_size)
        )

        return base64.b64encode(encrypted).decode("ascii")

    def decrypt(self, cipher_text: str) -> str:
        cipher = AES.new(self.key, AES.MODE_CBC, self.iv)

        decrypted = unpad(
            cipher.decrypt(base64.b64decode(cipher_text)),
            AES.block_size
        )

        return decrypted.decode("ascii")