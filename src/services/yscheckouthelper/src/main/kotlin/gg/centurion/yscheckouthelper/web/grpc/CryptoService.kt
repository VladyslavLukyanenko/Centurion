package gg.centurion.yscheckouthelper.web.grpc

import gg.centurion.checkout.CryptoGrpc.CryptoImplBase
import io.grpc.stub.StreamObserver
import gg.centurion.checkout.CryptoOuterClass.EncryptionResult
import gg.centurion.contract.Profile
import gg.centurion.yscheckouthelper.crypto.Adyen12
import gg.centurion.yscheckouthelper.crypto.JAdyen12

class CryptoService : CryptoImplBase() {
  override fun encrypt(request: Profile.ProfileData, responseObserver: StreamObserver<EncryptionResult>) {
    val encrypted = Adyen12().serializeAndEncryptProfile(request)
    responseObserver.onNext(EncryptionResult.newBuilder().setEncrypted(encrypted).build())
    responseObserver.onCompleted()
  }
}